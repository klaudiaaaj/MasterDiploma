using IronPython.Hosting;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace RabbitmqSubscriber.Services
{
    public class RosContractor
    {
        private readonly Uri uri = new Uri("ws://34.125.199.149:9090/");

        private const string pathToFunction = "C:/Users/klaud/Desktop/GazeboContractor.py";
        public async Task GazeboContractor(string dataString)
        {
            var data = dataString.Split(',');
            var time = float.Parse(data[0]);
            var axis_1 = float.Parse(data[1]);
            var axis_2 = float.Parse(data[2]);
            var button_1 = int.Parse(data[3]);
            var button_2 = int.Parse(data[4]);

            using (var client = new ClientWebSocket())
            {
                await client.ConnectAsync(uri, CancellationToken.None);

                // subscribe to topic
                var subscribeMsg = new
                {
                    op = "subscribe",
                    topic = "/cmd_vel",
                    type = "geometry_msgs/Twist"
                };
                Console.WriteLine($"Sent Twist message to /cmd_vel with linear x = {axis_1} and angular z = {axis_2}");
                var subscribeMsgBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(subscribeMsg));
                await client.SendAsync(new ArraySegment<byte>(subscribeMsgBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                // publish message
                var msgMsg = new
                {
                    op = "publish",
                    topic = "/cmd_vel",
                    msg = new
                    {
                        linear = new
                        {
                            x = axis_1,
                            y = 0.0,
                            z = 0.0
                        },
                        angular = new
                        {
                            x = 0.0,
                            y = 0.0,
                            z = axis_2
                        }
                    }
                };
                var msgMsgBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msgMsg));
                await client.SendAsync(new ArraySegment<byte>(msgMsgBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task GetVelocityData(string dataString)
        {
            using (var client = new ClientWebSocket())
            {
                await client.ConnectAsync(uri, CancellationToken.None);

                // subscribe to topic
                var subscribeMsg = new
                {
                    op = "subscribe",
                    topic = "husky_velocity_controller/odom",
                    type = "geometry_msgs/Twist"
                };

               // client.ReceiveAsync
            }
        }

            public async Task CallTheFunctionAsync(string parameters)
        {
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var scriptSource = engine.CreateScriptSourceFromFile(pathToFunction);

            // Set the value of the "data_string" variable in the Python script
            // scope.SetVariable("data_string", parameters);

            try
            {
                // Execute the Python script asynchronously
                var task = Task.Factory.StartNew(
                    () => engine.Execute(scriptSource.GetCode(), scope),
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskScheduler.Default);

                // Wait for the Python script to complete
                await task;

                // Get the value of the "parameter" variable from the Python script
                parameters = scope.GetVariable<string>("parameter");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
