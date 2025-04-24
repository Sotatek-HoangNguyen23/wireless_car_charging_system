using System.Data.SqlClient;
using System.IO.Ports;

namespace _Arduino_Barie
{
    internal class Program
    {
        private static string connectionString = "Server=HUETAYY\\SQLEXPRESS;Database=WCCS;User Id=sa;Password=123;";
        private static SerialPort arduinoPort;

        static void Main(string[] args)
        {
            arduinoPort = new SerialPort("COM4", 9600);
            arduinoPort.Open();
            Console.WriteLine("Waiting for command from Arduino...");

            while (true)
            {
                if (arduinoPort.BytesToRead > 0)
                {
                    string command = arduinoPort.ReadLine().Trim();
                    if (command.StartsWith("UPDATE charging_point"))
                    {
                        UpdateSlotStatus(command);
                    }
                    else
                    {
                        string licensePlate_raw = command;
                        int startIndex = licensePlate_raw.IndexOf("[licensePlate] ") + "[licensePlate] ".Length;
                        string licensePlate = licensePlate_raw.Substring(startIndex);
                        licensePlate = licensePlate.TrimEnd('}');
                        Console.WriteLine("Received License Plate:" + licensePlate);

                        if (IsVehicleRegistered(licensePlate))
                        {
                            Console.WriteLine("Vehicle is registered. Opening barrier.");
                            arduinoPort.Write("OPEN");
                        }
                        else
                        {
                            Console.WriteLine("Vehicle not registered.");
                            arduinoPort.Write("CLOSE");
                        }
                    }
                }
            }

        }

        public static bool IsVehicleRegistered(string licensePlate)
        {

            string query = "SELECT COUNT(*) FROM car WHERE license_plate = @LicensePlate";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LicensePlate", licensePlate);
                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }

        public static void UpdateSlotStatus(string query)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Query executed successfully: " + query);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error executing query: " + ex.Message);
            }
        }
    }
}
