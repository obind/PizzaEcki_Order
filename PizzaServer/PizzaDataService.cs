using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using System.Data;
using System.Data.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PizzaServer
{
    public class PizzaDataService
    {
        private readonly PizzaDbContext _context;

        public PizzaDataService(PizzaDbContext context)
        {
            _context = context;
        }



        public async Task<List<Driver>> GetAllDriversAsync()
        {
            return await _context.Drivers.ToListAsync();
        }


        public async Task SaveOrderAssignmentAsync(string orderId, int driverId, double price)
        {
            var connection = (SqliteConnection)_context.Database.GetDbConnection();
            await connection.OpenAsync();

            string checkSql = "SELECT COUNT(*) FROM OrderAssignments WHERE OrderId = @OrderId";
            using (SqliteCommand checkCommand = new SqliteCommand(checkSql, connection))
            {
                checkCommand.Parameters.AddWithValue("@OrderId", orderId);
                int count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                if (count > 0)
                {
                    string updateSql = "UPDATE OrderAssignments SET DriverId = @DriverId, Price = @Price, Timestamp = @Timestamp WHERE OrderId = @OrderId";
                    using (SqliteCommand updateCommand = new SqliteCommand(updateSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@OrderId", orderId);
                        updateCommand.Parameters.AddWithValue("@DriverId", driverId);
                        updateCommand.Parameters.AddWithValue("@Price", price);
                        updateCommand.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToString("yyyy-MM-dd"));
                        await updateCommand.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    string insertSql = "INSERT INTO OrderAssignments (OrderId, DriverId, Price, Timestamp) VALUES (@OrderId, @DriverId, @Price, @Timestamp)";
                    using (SqliteCommand insertCommand = new SqliteCommand(insertSql, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@OrderId", orderId);
                        insertCommand.Parameters.AddWithValue("@DriverId", driverId);
                        insertCommand.Parameters.AddWithValue("@Price", price);
                        insertCommand.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToString("yyyy-MM-dd"));
                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }
            }

            await connection.CloseAsync();
        }


        public async Task<List<Order>> GetUnassignedOrdersAsync()
        {
            List<Order> unassignedOrders = new List<Order>();

            var connection = (SqliteConnection)_context.Database.GetDbConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT 
                    Orders.*,
                    OrderItems.*
                FROM 
                    Orders
                LEFT JOIN 
                    OrderItems ON Orders.OrderId = OrderItems.OrderId
                LEFT JOIN 
                    OrderAssignments ON Orders.OrderId = OrderAssignments.OrderId
                WHERE 
                    OrderAssignments.DriverId IS NULL
            ";

            using (SqliteCommand command = new SqliteCommand(sql, connection))
            {
                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var orderIdValue = reader["OrderId"].ToString();
                        if (string.IsNullOrEmpty(orderIdValue))
                        {
                            var bonNumber = reader["BonNumber"].ToString();
                            Console.WriteLine($"Fehler: OrderId ist null oder leer für BonNumber: {bonNumber}");
                            continue;  // Überspringe diesen Datensatz
                        }
                        Guid currentOrderId = Guid.Parse(orderIdValue);

                        Order order;
                        if (unassignedOrders.Any(o => o.OrderId == currentOrderId))
                        {
                            order = unassignedOrders.First(o => o.OrderId == currentOrderId);
                        }
                        else
                        {
                            // Hier nimmst du die Daten für IsDelivery aus der Datenbank
                            var isDeliveryValue = reader["IsDelivery"];
                            bool isDelivery = false;

                            // Wenn der Wert aus der Datenbank kommt, musst du ihn entsprechend konvertieren.
                            if (isDeliveryValue != DBNull.Value)
                            {
                                isDelivery = Convert.ToInt32(isDeliveryValue) != 0;
                            }

                            order = new Order
                            {
                                OrderId = currentOrderId,
                                BonNumber = Convert.ToInt32(reader["BonNumber"]),
                                IsDelivery = isDelivery,
                                PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                CustomerPhoneNumber = reader.IsDBNull(reader.GetOrdinal("CustomerPhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("CustomerPhoneNumber")),
                                Timestamp = reader.IsDBNull(reader.GetOrdinal("Timestamp")) ? null : reader.GetString(reader.GetOrdinal("Timestamp")),
                                DeliveryUntil = reader.IsDBNull(reader.GetOrdinal("DeliveryUntil")) ? null : reader.GetString(reader.GetOrdinal("DeliveryUntil")),
                            };
                            unassignedOrders.Add(order);
                        }

                        OrderItem orderItem = new OrderItem
                        {
                            Nr = reader.IsDBNull(reader.GetOrdinal("OrderItemId")) ? 0 : reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                            Gericht = reader.IsDBNull(reader.GetOrdinal("Gericht")) ? null : reader.GetString(reader.GetOrdinal("Gericht")),
                            Extras = reader.IsDBNull(reader.GetOrdinal("Extras")) ? null : reader.GetString(reader.GetOrdinal("Extras")),
                            Größe = reader.IsDBNull(reader.GetOrdinal("Größe")) ? null : reader.GetString(reader.GetOrdinal("Größe")),
                            Menge = reader.IsDBNull(reader.GetOrdinal("Menge")) ? 0 : reader.GetInt32(reader.GetOrdinal("Menge")),
                            Epreis = reader.IsDBNull(reader.GetOrdinal("Epreis")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Epreis")),
                            Gesamt = reader.IsDBNull(reader.GetOrdinal("Gesamt")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Gesamt")),
                            Uhrzeit = reader.IsDBNull(reader.GetOrdinal("Uhrzeit")) ? null : reader.GetString(reader.GetOrdinal("Uhrzeit")),
                            LieferungsArt = reader.IsDBNull(reader.GetOrdinal("LieferungsArt")) ? 0 : reader.GetInt32(reader.GetOrdinal("LieferungsArt"))
                        };


                    order.OrderItems.Add(orderItem);
                }
            }
        }

        await connection.CloseAsync();

            return unassignedOrders;
        }

        public async Task<bool> DeleteOrderAsync(Guid orderId)
        {
            var connection = (SqliteConnection)_context.Database.GetDbConnection();
            try
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    int result = 0;

                    // Lösche zugehörige Einträge in OrderItems
                    string sqlDeleteOrderItems = "DELETE FROM OrderItems WHERE OrderId = @OrderId;";
                    using (var command = new SqliteCommand(sqlDeleteOrderItems, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId.ToString());
                        result += await command.ExecuteNonQueryAsync();
                    }

                    // Lösche zugehörige Einträge in OrderAssignments
                    string sqlDeleteAssignments = "DELETE FROM OrderAssignments WHERE OrderId = @OrderId;";
                    using (var command = new SqliteCommand(sqlDeleteAssignments, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId.ToString());
                        result += await command.ExecuteNonQueryAsync();
                    }

                    // Lösche den Eintrag in der Orders-Tabelle
                    string sqlDeleteOrder = "DELETE FROM Orders WHERE OrderId = @OrderId;";
                    using (var command = new SqliteCommand(sqlDeleteOrder, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId.ToString());
                        result += await command.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();

                    // Prüfe, ob irgendeine Zeile betroffen war
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                // Loggen Sie die Ausnahme oder handeln Sie sie entsprechend.
                // Zum Beispiel:
                // _logger.LogError(ex, "An error occurred while deleting order with ID {OrderId}", orderId);
                return false;
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }



        public async Task<Customer> GetCustomerByPhoneNumber(string phoneNumber)
        {
            string sql = @"SELECT c.PhoneNumber, c.Name, a.Street, a.City, c.AdditionalInfo 
                   FROM Customers c
                   INNER JOIN Addresses a ON c.AddressId = a.Id
                   WHERE c.PhoneNumber = @PhoneNumber";

            var connection = (SqliteConnection)_context.Database.GetDbConnection();
            await connection.OpenAsync();

            using (SqliteCommand command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        return new Customer
                        {
                            PhoneNumber = reader.GetString(0),
                            Name = reader.GetString(1),
                            Street = reader.GetString(2),
                            City = reader.IsDBNull(3) ? null : reader.GetString(3), // Überprüfe auf NULL
                            AdditionalInfo = reader.IsDBNull(4) ? null : reader.GetString(4) // Überprüfe auf NULL
                        };
                    }
                }
            }

            return null;
        }


    }
}