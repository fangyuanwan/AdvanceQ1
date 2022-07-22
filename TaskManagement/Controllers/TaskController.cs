using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Models;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors;

namespace TaskManagement.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly TaskDBContext _context;

        public TaskController(TaskDBContext context)
        {
            _context = context;
        }

        // GET: api/Task
        [EnableCors("MyPolicy")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<task>>> GetTasks()
        {
            return await _context.Tasks.ToListAsync();
        }

        // GET: api/Task/5
        [EnableCors("MyPolicy")]
        [HttpGet("{id}")]
        public async Task<ActionResult<task>> Gettask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            return task;
        }

        // PUT: api/Task/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [EnableCors("MyPolicy")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Puttask(int id, task task)
        {
            if (id != task.taskID)
            {
                return BadRequest();
            }

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!taskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Task
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [EnableCors("MyPolicy")]
        [HttpPost]
        public async Task<ActionResult<task>> Posttask(task task)
        {
            _context.Tasks.Add(task);
            
            var factory = new ConnectionFactory()
            {
                //HostName = "localhost" , 
                //Port = 30724
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
            };

            Console.WriteLine(factory.HostName + ":" + factory.Port);
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "tasks",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

              
                var message = JsonConvert.SerializeObject(task);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "",
                                     routingKey: "tasks",
                                     basicProperties: null,
                                     body: body);
            }
            await _context.SaveChangesAsync();
            return CreatedAtAction("Gettask", new { id = task.taskID }, task);
        }

        // DELETE: api/Task/5
        [EnableCors("MyPolicy")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletetask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool taskExists(int id)
        {
            return _context.Tasks.Any(e => e.taskID == id);
        }
    }
}
