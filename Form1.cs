using System.Net;
using System.Text;

namespace battery_status_form
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            startMainThread();
        }

        public void startMainThread() {
            Thread serverThread = new Thread(startHttpServer);
            serverThread.IsBackground = true; // Ensure the thread closes when the main application exits
            serverThread.Start();
        }

        static void startHttpServer()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:3300/");
            listener.Start();
            Console.WriteLine("HTTP server started on http://localhost:3300/");

            while (true)
            {
                try
                {
                    // Wait for a request
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    // Add CORS headers to the response
                    response.AddHeader("Access-Control-Allow-Origin", "*"); // Allow all origins
                    response.AddHeader("Access-Control-Allow-Methods", "GET");
                    response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

                    // Handle OPTIONS request for preflight
                    if (request.HttpMethod == "OPTIONS")
                    {
                        response.StatusCode = 204; // No Content
                        response.Close();
                        continue;
                    }

                    // Route handling
                    if (request.Url.AbsolutePath == "/battery-status-percentage")
                    {
                        // Get battery percentage
                        int batteryPercentage = (int)(SystemInformation.PowerStatus.BatteryLifePercent * 100);
                        String batteryChargeStatus = SystemInformation.PowerStatus.BatteryChargeStatus.ToString();
                        String batteryLineStatus = SystemInformation.PowerStatus.PowerLineStatus.ToString();
                        String batteryLifeRemaining = SystemInformation.PowerStatus.BatteryLifeRemaining.ToString();

                        // Respond with the battery percentage
                        string responseString = $"{{\"batteryPercentage\": {batteryPercentage}," +
                            $"\"batteryChargeStatus\": \"{batteryChargeStatus}\"," +
                            $"\"batteryLineStatus\": \"{batteryLineStatus}\"," +
                            $"\"batteryLifeRemaining\": \"{batteryLifeRemaining}\"}}";
                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                        response.ContentType = "application/json";
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        // Handle 404
                        response.StatusCode = 404;
                        string responseString = "Endpoint not found";
                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }

                    response.OutputStream.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_DoubleClick(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}