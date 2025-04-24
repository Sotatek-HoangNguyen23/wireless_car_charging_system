using System.Data.SqlClient;
using System.IO.Ports;
using System.Windows.Input;

namespace _Arduino_ChargingPoint1
{
    internal class Program
    {
        private static string myCnn = "Server=HUETAYY\\SQLEXPRESS;Database=WCCS;User Id=sa;Password=123";
        private static SerialPort arduinoPort;
        private static string license_plate = null;
        private static string car_id = null;
        private static string user_id = null;
        private static bool hasReceivedLicensePlate = false;
        private static DateTime? start_time;
        private static bool hasAddToDatabase2 = false;

        class ChargingSession
        {
            public string ChargingPointId { get; set; }
            public string BatteryPercentage { get; set; }
            public string BatteryVoltage { get; set; }
            public string ChargingCurrent { get; set; }
            public string Temperature { get; set; }
            public string ChargingPower { get; set; }
            public string FullPower { get; set; }
            public string ChargingTime { get; set; }
            public string EnergyConsumed { get; set; }
            public string Cost { get; set; }

            public bool IsComplete()
            {
                return !string.IsNullOrEmpty(ChargingPointId) && !string.IsNullOrEmpty(BatteryPercentage) && !string.IsNullOrEmpty(BatteryVoltage) &&
                        !string.IsNullOrEmpty(ChargingCurrent) && !string.IsNullOrEmpty(Temperature) &&
                       !string.IsNullOrEmpty(ChargingPower) && !string.IsNullOrEmpty(FullPower) && !string.IsNullOrEmpty(ChargingTime) &&
                       !string.IsNullOrEmpty(EnergyConsumed) && !string.IsNullOrEmpty(Cost);
            }
        }

        class ChargingSession2
        {
            public string CarId { get; set; }
            public string ChargingPointId { get; set; }
            public string UserId { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string EnergyConsumed { get; set; }
            public string Cost { get; set; }
            public string status { get; set; }

        }

        static void Main(string[] args)
        {
        restart_program:
            arduinoPort = new SerialPort("COM11", 9600);
            arduinoPort.Open();
            Console.WriteLine("Waiting for command from Arduino...");
            try
            {
                while (true)
                {
                    string receivedLine = arduinoPort.ReadLine().Trim();
                    if (receivedLine != null)
                    {
                        if (receivedLine.Contains("[licensePlate]") && hasReceivedLicensePlate == false)
                        {
                            string command = arduinoPort.ReadLine().Trim();
                            license_plate = ExtractPlateNumber(command);
                            if (license_plate != null)
                            {
                                car_id = GetCarId(license_plate);
                                if (car_id != null)
                                {
                                    user_id = GetUserIdByCarId(car_id);
                                    if (user_id != null)
                                    {
                                        string getInfo = GetOwnerAndBalance(user_id);

                                        bool checkBalance = HasBalance(user_id);
                                        hasReceivedLicensePlate = true;

                                        if (getInfo != null)
                                        {
                                            if (checkBalance == true)
                                            {
                                                arduinoPort.Write(getInfo);
                                                Console.WriteLine(getInfo);
                                            }
                                            else
                                            {
                                                arduinoPort.Write("1");
                                                Console.WriteLine("Your balance is zero!");
                                                goto restart_program;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (receivedLine.Contains("[disconnected]"))
                        {
                            start_time = null;
                            hasAddToDatabase2 = false;
                            license_plate = null;
                            car_id = null;
                            user_id = null;
                            hasReceivedLicensePlate = false;
                            goto restart_program;
                        }

                        if (receivedLine.Contains("[charging]"))
                        {
                            start_time = DateTime.Now;
                            if (HasBalance(user_id) == true)
                            {
                                ChargingSession session = new ChargingSession();
                                hasReceivedLicensePlate = false;
                                try
                                {
                                    while (true)
                                    {
                                        if (HasBalance(user_id) == true)
                                        {
                                            try
                                            {
                                                string receivedData = arduinoPort.ReadLine();
                                                Console.WriteLine(receivedData);
                                                ProcessReceivedData(receivedData, session);
                                                if (!receivedData.Contains("[disconnected]"))
                                                {
                                                    Console.WriteLine($"Charging Point Id: {session.ChargingPointId}");
                                                    Console.WriteLine($"Battery Percentage: {session.BatteryPercentage}");
                                                    Console.WriteLine($"Battery Voltage: {session.BatteryVoltage}");
                                                    Console.WriteLine($"Charging Current: {session.ChargingCurrent}");
                                                    Console.WriteLine($"Temperature: {session.Temperature}");
                                                    Console.WriteLine($"Charging Power: {session.ChargingPower}");
                                                    Console.WriteLine($"Full Power: {session.FullPower}");
                                                    Console.WriteLine($"Charging Time: {session.ChargingTime}");
                                                    Console.WriteLine($"Energy Consumed: {session.EnergyConsumed}");
                                                    Console.WriteLine($"Cost: {session.Cost}");

                                                    if (session.IsComplete())
                                                    {
                                                        Console.WriteLine("Session is complete, adding to database...");
                                                        AddToDatabase(session);
                                                        if (hasAddToDatabase2 == false)
                                                        {
                                                            AddToDatabase2(session.ChargingPointId, session.EnergyConsumed, session.Cost);
                                                            hasAddToDatabase2 = true;
                                                        }
                                                        if (hasAddToDatabase2 == true)
                                                        {
                                                            UpdateToDatabase2(session.EnergyConsumed, session.Cost);
                                                        }
                                                        UpdateBalance(GetTOP1StepCost(), user_id);
                                                        session = new ChargingSession();
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Session not complete, waiting for more data...");
                                                    }
                                                }
                                                else
                                                {
                                                    start_time = null;
                                                    hasAddToDatabase2 = false;
                                                    disconnectStatus();
                                                    disconnectStatus2();
                                                    license_plate = null;
                                                    car_id = null;
                                                    user_id = null;
                                                    hasReceivedLicensePlate = false;
                                                    goto restart_program;
                                                }
                                            }
                                            catch (TimeoutException)
                                            {
                                                Console.WriteLine("Timeout occurred while reading from the serial port.");
                                            }
                                        }
                                        else
                                        {
                                            start_time = null;
                                            hasAddToDatabase2 = false;
                                            arduinoPort.Write("0");
                                            Console.WriteLine("Out of money!");
                                            disconnectStatus();
                                            disconnectStatus2();
                                            hasReceivedLicensePlate = false;
                                            goto restart_program;
                                        }
                                    }
                                }
                                catch (IOException ex)
                                {
                                    Console.WriteLine("Error opening or communicating with the serial port: " + ex.Message);
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    Console.WriteLine("Access to the serial port is denied. Ensure the port is not being used by another application.");
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading from the serial port: " + ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access to the serial port is denied.");
            }
            finally
            {
                if (arduinoPort.IsOpen)
                {
                    arduinoPort.Close();
                }
            }
        }

        static void ProcessReceivedData(string data, ChargingSession session)
        {
            if (data.Contains("ChrgPoint ID"))
            {
                session.ChargingPointId = ExtractValueAsString(data);
            }
            if (data.Contains("Battery Percentage"))
            {
                session.BatteryPercentage = ExtractValueAsString(data);
            }
            else if (data.Contains("Voltage"))
            {
                session.BatteryVoltage = ExtractValueAsString(data);
            }
            else if (data.Contains("Current"))
            {
                session.ChargingCurrent = ExtractValueAsString(data);
            }
            else if (data.Contains("Temperature"))
            {
                session.Temperature = ExtractValueAsString(data);
            }
            else if (data.Contains("Power"))
            {
                session.ChargingPower = ExtractValueAsString(data);
            }
            else if (data.Contains("FullErg"))
            {
                session.FullPower = ExtractValueAsString(data);
            }
            else if (data.Contains("Time"))
            {
                session.ChargingTime = ExtractValueAsString(data);
            }
            else if (data.Contains("Energy Consumed"))
            {
                session.EnergyConsumed = ExtractValueAsString(data);
            }
            else if (data.Contains("Cost"))
            {
                session.Cost = ExtractValueAsString(data);
            }
        }

        static string ExtractValueAsString(string data)
        {
            int colonIndex = data.IndexOf(':');
            if (colonIndex != -1)
            {
                string value = data.Substring(colonIndex + 1).Trim();
                value = value.TrimEnd(new char[] { 'A', 'V', 'W', '%', 'C', ' ', 'V', 'N', 'D', 'h', 'm' });
                return value;
            }
            return string.Empty;
        }

        static string ExtractPlateNumber(string input)
        {
            int startIndex = input.IndexOf("[licensePlate]") + "[licensePlate]".Length;
            string extractedPlate = input.Substring(startIndex).Trim();
            extractedPlate = extractedPlate.Trim(new char[] { '{', '}', ' ' });
            return extractedPlate;
        }

        static string GetUserIdByCarId(string car_id)
        {
            string connectionString = myCnn;
            string userId = "-1";

            string query = @"
    SELECT TOP 1 uc.user_id
    FROM user_car uc
    JOIN users u ON uc.user_id = u.user_id
    WHERE uc.car_id = @CarId AND uc.IsAllowedToCharge = 1
    ORDER BY 
        CASE 
            WHEN uc.Role = 'Renter' THEN 1  
            WHEN uc.Role = 'Owner' THEN 2  
            ELSE 3 
        END";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CarId", car_id);
                        var result = command.ExecuteScalar();

                        if (result != null && int.TryParse(result.ToString(), out int parsedUserId))
                        {
                            userId = parsedUserId.ToString();
                        }
                        else
                        {
                            userId = "-1";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xảy ra: {ex.Message}");
                userId = "-1";
            }
            return userId;
        }

        static void AddToDatabase(ChargingSession session)
        {
            string car_id = GetCarId(license_plate);
            float step_cost = float.Parse(session.ChargingPower) / 1000 * 16 / 3600 * 4000;
            string connectionString = myCnn;
            string query = "INSERT INTO real_time_data (chargingpoint_id, car_id, license_plate, battery_level, battery_voltage, charging_current, temperature, charging_power, powerpoint, charging_time, energy_consumed, step_cost, cost, start_time, status) " +
                           "VALUES (@ChargingPointID,@CarId, @LicensePlate ,@BatteryPercentage, @BatteryVoltage,  @ChargingCurrent, @Temperature, @ChargingPower, @FullPower, @ChargingTime, @EnergyConsumed, @StepCost, @Cost, @StartTime, @Status)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ChargingPointID", Convert.ToInt32(session.ChargingPointId));
                command.Parameters.AddWithValue("@CarId", Convert.ToInt32(car_id));
                command.Parameters.AddWithValue("@LicensePlate", license_plate);
                command.Parameters.AddWithValue("@BatteryPercentage", session.BatteryPercentage);
                command.Parameters.AddWithValue("@BatteryVoltage", session.BatteryVoltage);
                command.Parameters.AddWithValue("@ChargingCurrent", session.ChargingCurrent);
                command.Parameters.AddWithValue("@Temperature", session.Temperature);
                command.Parameters.AddWithValue("@ChargingPower", session.ChargingPower);
                command.Parameters.AddWithValue("@FullPower", session.FullPower);
                command.Parameters.AddWithValue("@ChargingTime", session.ChargingTime);
                command.Parameters.AddWithValue("@EnergyConsumed", session.EnergyConsumed);
                command.Parameters.AddWithValue("@StepCost", step_cost.ToString());
                command.Parameters.AddWithValue("@Cost", session.Cost);
                command.Parameters.AddWithValue("@StartTime", start_time);
                command.Parameters.AddWithValue("@Status", "Charging");

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} row(s) inserted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        static void AddToDatabase2(string chargingpoint_id, string energy_consumed, string cost)
        {
            string car_id = GetCarId(license_plate);
            string connectionString = myCnn;
            string query = "INSERT INTO charging_session (car_id, charging_point_id, user_id, start_time, energy_consumed, cost, status) " +
                           "VALUES (@CarId, @ChargingPointID, @UserId, @StartTime, @EnergyConsumed, @Cost, @Status)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CarId", Convert.ToInt32(car_id));
                command.Parameters.AddWithValue("@ChargingPointID", Convert.ToInt32(chargingpoint_id));
                command.Parameters.AddWithValue("@UserId", Convert.ToInt32(user_id));
                command.Parameters.AddWithValue("@StartTime", start_time);
                command.Parameters.AddWithValue("@EnergyConsumed", energy_consumed);
                command.Parameters.AddWithValue("@Cost", cost);
                command.Parameters.AddWithValue("@Status", "Charging");

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} row(s) inserted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        static void UpdateToDatabase2(string energy_consumed, string cost)
        {
            string connectionString = myCnn;
            string car_id = GetCarId(license_plate);

            string selectQuery = @"
    SELECT TOP 1 session_id 
    FROM charging_session
    WHERE car_id = @CarId AND user_id = @UserId
    ORDER BY ABS(DATEDIFF(SECOND, start_time, GETDATE())) ASC";

            string updateQuery = "UPDATE charging_session SET energy_consumed = @EnergyConsumed, cost = @Cost WHERE session_id = @Id AND status = 'Charging'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
                    selectCommand.Parameters.AddWithValue("@CarId", car_id);
                    selectCommand.Parameters.AddWithValue("@UserId", user_id);

                    SqlDataReader reader = selectCommand.ExecuteReader();

                    if (reader.Read())
                    {
                        int recordId = Convert.ToInt32(reader["session_id"]);
                        reader.Close();

                        SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                        updateCommand.Parameters.AddWithValue("@EnergyConsumed", energy_consumed);
                        updateCommand.Parameters.AddWithValue("@Cost", cost);
                        updateCommand.Parameters.AddWithValue("@Id", recordId);
                        updateCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        Console.WriteLine("Not Found");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        static void disconnectStatus()
        {
            string connectionString = myCnn;
            string car_id = GetCarId(license_plate);
            string selectQuery = "SELECT TOP 1 data_id FROM real_time_data WHERE car_id = @CarId ORDER BY time_moment DESC";
            string updateQuery = "UPDATE real_time_data SET status = 'Disconnected' WHERE data_id = @Id";
            string updateQuery2 = "UPDATE real_time_data SET end_time = @Endtime WHERE data_id = @Id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
                    selectCommand.Parameters.AddWithValue("@CarId", car_id);

                    SqlDataReader reader = selectCommand.ExecuteReader();

                    if (reader.Read())
                    {
                        int recordId = Convert.ToInt32(reader["data_id"]);

                        reader.Close();
                        SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                        updateCommand.Parameters.AddWithValue("@Id", recordId);
                        updateCommand.ExecuteNonQuery();

                        SqlCommand updateCommand2 = new SqlCommand(updateQuery2, connection);
                        updateCommand2.Parameters.AddWithValue("@Endtime", DateTime.Now);
                        updateCommand2.Parameters.AddWithValue("@Id", recordId);
                        updateCommand2.ExecuteNonQuery();
                    }
                    else
                    {
                        Console.WriteLine("Not Found");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        static void disconnectStatus2()
        {
            string connectionString = myCnn;
            string car_id = GetCarId(license_plate);

            string selectQuery = @"
    SELECT TOP 1 session_id 
    FROM charging_session
    WHERE car_id = @CarId AND user_id = @UserId
    ORDER BY ABS(DATEDIFF(SECOND, start_time, GETDATE())) ASC";
            string updateQuery = "UPDATE charging_session SET end_time = @EndTime WHERE session_id = @Id";
            string updateQuery2 = "UPDATE charging_session SET status = 'Completed' WHERE session_id = @Id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
                    selectCommand.Parameters.AddWithValue("@CarId", car_id);
                    selectCommand.Parameters.AddWithValue("@UserId", user_id);

                    SqlDataReader reader = selectCommand.ExecuteReader();

                    if (reader.Read())
                    {
                        int recordId = Convert.ToInt32(reader["session_id"]);
                        reader.Close();

                        SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                        updateCommand.Parameters.AddWithValue("@EndTime", DateTime.Now);
                        updateCommand.Parameters.AddWithValue("@Id", recordId);
                        updateCommand.ExecuteNonQuery();

                        SqlCommand updateCommand2 = new SqlCommand(updateQuery2, connection);
                        updateCommand2.Parameters.AddWithValue("@Id", recordId);
                        updateCommand2.ExecuteNonQuery();
                    }
                    else
                    {
                        Console.WriteLine("Not Found");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        static string GetOwnerAndBalance(string user_id)
        {
            string connectionString = myCnn;
            string query = "SELECT u.fullname, b.balance\r\nFROM users u\r\nJOIN balance b ON u.user_id = b.user_id\r\nWHERE u.user_id = @userId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", user_id);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string fullname = reader["fullname"].ToString();
                        decimal balance = Convert.ToDecimal(reader["balance"]);

                        string[] nameParts = fullname.Split(' ');
                        string lastName = nameParts[nameParts.Length - 1];

                        return $"{lastName} - {balance}";
                    }
                    else
                    {
                        return "not found.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    return "Error fetching data.";
                }
            }
        }

        static string GetTOP1StepCost()
        {
            string connectionString = myCnn;
            string car_id = GetCarId(license_plate);
            string query = "SELECT TOP 1 step_cost FROM real_time_data WHERE car_id = @CarId ORDER BY time_moment DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CarId", car_id);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string step_cost = reader["step_cost"].ToString();

                        return step_cost;
                    }
                    else
                    {
                        return "Vehicle not found.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    return "Error fetching data.";
                }
            }
        }

        static void UpdateBalance(string step_cost, string user_id)
        {
            string connectionString = myCnn;
            try
            {
                string updateQuery = "UPDATE [balance] SET balance = balance - @step_cost, update_at = GETDATE() WHERE user_id = @userId";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@step_cost", step_cost);
                        cmd.Parameters.AddWithValue("@userId", user_id);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("Balance updated successfully.");
                        }
                        else
                        {
                            Console.WriteLine("No records found with the provided car ID.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        private static decimal BalanceThreshold = 1000m;

        static bool HasBalance(string user_id)
        {
            string connectionString = myCnn;
            string query = "SELECT balance FROM balance WHERE user_id = @userId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", user_id);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        decimal balance = Convert.ToDecimal(reader["balance"]);
                        return balance > BalanceThreshold;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    return false;
                }
            }
        }

        static string GetCarId(string licensePlate)
        {
            string connectionString = myCnn;
            string query = "SELECT car_id FROM car WHERE license_plate = @licensePlate AND is_deleted = 0";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@licensePlate", licensePlate);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        return reader["car_id"].ToString();
                    }
                    else
                    {
                        return "Vehicle not found.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    return "Error fetching data.";
                }
            }
        }
    }
}
