using System;
using MySql.Data.MySqlClient;
using Mysqlx.Prepare;

namespace Progra3Card.Administrativo
{
    class Program
    {


        private static string connectionString = "Server=localhost;Database=mi_banco_db;Uid=root;Pwd=;";

        static void Main(string[] args)
        {
            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("    SISTEMA ADMINISTRATIVO PROGRA3CARD   ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Emitir Nueva Tarjeta (Alta de Cliente)");
                Console.WriteLine("2. Listar Tarjetas");
                Console.WriteLine("3. Ver Detalle de una Tarjeta / Cliente");
                Console.WriteLine("4. Eliminar Tarjeta (Baja de Sistema)");
                Console.WriteLine("5. Emitir Nueva Liquidación Mensual");
                Console.WriteLine("6. Salir");
                Console.WriteLine("========================================");
                Console.Write("Seleccione una opción: ");

                switch (Console.ReadLine())
                {
                    case "1": MenuEmitirTarjeta(); break;
                    case "2": MenuListarTarjetas(); break;
                    case "3": MenuVerDetalleTarjeta(); break;
                    case "4": MenuEliminarTarjeta(); break;
                    case "5": MenuEmitirLiquidacion(); break;
                    case "6": salir = true; break;
                    default:
                        Console.WriteLine("Opción no válida. Presione una tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }
        }


        // Funciones a completar:

        static void MenuListarTarjetas()
        {
            Console.Clear();
            Console.WriteLine("--- LISTADO GENERAL DE TARJETAS ---");
            Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15}", "Nro Cuenta", "Nro Tarjeta", "Banco Emisor", "DNI Titular");
            Console.WriteLine("----------------------------------------------------------------------");

            // === A realizar ===
            // Aquí deben implementar un SELECT sobre la tabla 'tarjetas'
            // para recorrer las filas e imprimirlas en la consola.



            ObtenerYMostrarTarjetas();

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }




        static void MenuVerDetalleTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- DETALLE DE TARJETA Y CLIENTE ---");
            Console.Write("Ingrese el Número de Cuenta a consultar: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            // === A realizar ===
            // Aquí deben realizar un SELECT con un JOIN entre 'tarjetas' y 'usuarios' 
            // filtrando por el numCuenta para traer todos los campos (Nombre, Apellido, Email, Saldo, etc.)

            MostrarDetalleCompleto(numCuenta);

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuEliminarTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- ELIMINAR TARJETA DEL SISTEMA ---");
            Console.Write("Ingrese el Número de Cuenta de la tarjeta a dar de baja: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n⚠️ ADVERTENCIA: Se eliminará la tarjeta, sus liquidaciones y los datos de acceso web vinculados.");
            Console.ResetColor();
            Console.Write("¿Está seguro de continuar? (S/N): ");

            if (Console.ReadLine().ToUpper() == "S")
            {
                // === A realizar ===
                // Aquí deben ejecutar un DELETE sobre la tabla 'tarjetas' donde num_cuenta = numCuenta.
                // Como definimos ON DELETE CASCADE en la base de datos, las liquidaciones se borrarán solas.
                // Opcional: Evaluar si también eliminan al usuario de la tabla 'usuarios' o si lo mantienen.

                bool exito = DarDeBajaTarjeta(numCuenta);

                if (exito)
                    Console.WriteLine("\nTarjeta eliminada correctamente del sistema.");
                else
                    Console.WriteLine("\nError al intentar eliminar la tarjeta. Verifique el número de cuenta.");
            }
            else
            {
                Console.WriteLine("\nOperación cancelada.");
            }

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }
        //Menu para emitir liquidacion
        static void MenuEmitirTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- EMITIR NUEVA TARJETA (ALTA DE CLIENTE) ---");

            // 1. Solicito los datos del usuario
            Console.Write("Ingrese DNI/Documento del cliente: ");
            string dni = Console.ReadLine();
            Console.Write("Ingrese Nombre: ");
            string nombre = Console.ReadLine();
            Console.Write("Ingrese Apellido: ");
            string apellido = Console.ReadLine();
            Console.Write("Ingrese Email: ");
            string email = Console.ReadLine();

            // 2. Solicito los datos de la tarjeta
            Console.Write("Ingrese el Número de Tarjeta (16 dígitos): ");
            string nroTarjeta = Console.ReadLine();
            Console.Write("Ingrese el Banco Emisor (Ej: Banco Galicia, Banco Nación): ");
            string banco = Console.ReadLine();

           
            bool exito = RegistrarClienteYTarjeta(dni, nombre, apellido, email, nroTarjeta, banco);

            if (exito)
                Console.WriteLine("\nCliente y Tarjeta registrados con éxito en el sistema.");
            else
                Console.WriteLine("\nHubo un error al registrar. Verifique si el DNI o la tarjeta ya existen.");

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }


        static bool RegistrarClienteYTarjeta(string dni, string nombre, string apellido, string email, string nroTarjeta, string banco)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // --- PASO 1: Insertar el usuario ---
                    
                    string queryUsuario = "INSERT INTO usuarios (documento, nombre, apellido, email, usuario, password) " +
                                          "VALUES (@dni, @nombre, @apellido, @email, NULL, NULL)";

                    MySqlCommand cmdUsuario = new MySqlCommand(queryUsuario, conn);
                    cmdUsuario.Parameters.AddWithValue("@dni", dni);
                    cmdUsuario.Parameters.AddWithValue("@nombre", nombre);
                    cmdUsuario.Parameters.AddWithValue("@apellido", apellido);
                    cmdUsuario.Parameters.AddWithValue("@email", email);

                    cmdUsuario.ExecuteNonQuery(); 

                    // --- PASO 2: Insertar la tarjeta ---
                    
                    string queryTarjeta = "INSERT INTO tarjetas (numero_tarjeta, banco_emisor, saldo, estado, dni_titular) " +
                                          "VALUES (@nroTarjeta, @banco, 0, 'Activa', @dni)";

                    MySqlCommand cmdTarjeta = new MySqlCommand(queryTarjeta, conn);
                    cmdTarjeta.Parameters.AddWithValue("@nroTarjeta", nroTarjeta);
                    cmdTarjeta.Parameters.AddWithValue("@banco", banco);
                    cmdTarjeta.Parameters.AddWithValue("@dni", dni); 

                    cmdTarjeta.ExecuteNonQuery(); 

                    return true; 
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError en la base de datos: " + ex.Message);
                    return false;
                }
            }
        }
        //Menu para emitir liquidacion

        static void MenuEmitirLiquidacion()
        {
            Console.Clear();
            Console.WriteLine("--- EMITIR NUEVA LIQUIDACIÓN MENSUAL ---");

            Console.Write("Ingrese el Número de Cuenta de la tarjeta: ");
            int cuenta = Convert.ToInt32(Console.ReadLine());
            Console.Write("Ingrese el Periodo (Formato YYYY-MM): ");
            string periodo = Console.ReadLine();
            Console.Write("Ingrese Fecha de Vencimiento (Formato YYYY-MM-DD): ");
            string vencimiento = Console.ReadLine();
            Console.Write("Ingrese el Total a Pagar: ");
            decimal total = Convert.ToDecimal(Console.ReadLine());
            Console.Write("Ingrese el Pago Mínimo: ");
            decimal minimo = Convert.ToDecimal(Console.ReadLine());

            bool exito = RegistrarLiquidacion(cuenta, periodo, vencimiento, total, minimo);

            if (exito)
                Console.WriteLine("\nLiquidación emitida correctamente.");
            else
                Console.WriteLine("\nError al emitir la liquidación. Verifique el número de cuenta.");

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static bool RegistrarLiquidacion(int cuenta, string periodo, string vencimiento, decimal total, decimal minimo)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                
                string query = "INSERT INTO liquidaciones (periodo, fecha_vencimiento, total_a_pagar, pago_minimo, num_cuenta) " +
                               "VALUES (@periodo, @vencimiento, @total, @minimo, @cuenta)";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@periodo", periodo);
                cmd.Parameters.AddWithValue("@vencimiento", vencimiento);
                cmd.Parameters.AddWithValue("@total", total);
                cmd.Parameters.AddWithValue("@minimo", minimo);
                cmd.Parameters.AddWithValue("@cuenta", cuenta);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError al insertar en liquidaciones: " + ex.Message);
                    return false;
                }
            }
        }



        // =========================================================================
        // MÉTODOS BASE QUE DEBEN COMPLETAR CON LA LÓGICA 
        // =========================================================================

        static void ObtenerYMostrarTarjetas()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT num_cuenta, numero_tarjeta, banco_emisor, dni_titular FROM tarjetas";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                try
                {
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15}",
                                reader["num_cuenta"].ToString(),
                                reader["numero_tarjeta"].ToString(),
                                reader["banco_emisor"].ToString(),
                                reader["dni_titular"].ToString()
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError al cargar las tarjetas: " + ex.Message);
                }
            }
        }

        static void MostrarDetalleCompleto(int cuenta)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT u.nombre, u.apellido, u.email, t.numero_tarjeta, t.banco_emisor, t.saldo, t.estado " +
                               "FROM tarjetas t " +
                               "INNER JOIN usuarios u ON t.dni_titular = u.documento " +
                               "WHERE t.num_cuenta = @cuenta";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cuenta", cuenta);

                try
                {
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine("\n=== DATOS DEL TITULAR ===");
                            Console.WriteLine($"Nombre Completo: {reader["apellido"]}, {reader["nombre"]}");
                            Console.WriteLine($"Email:           {reader["email"]}");

                            Console.WriteLine("\n=== DATOS DE LA TARJETA ===");
                            Console.WriteLine($"Número de Tarjeta: {reader["numero_tarjeta"]}");
                            Console.WriteLine($"Banco Emisor:      {reader["banco_emisor"]}");
                            Console.WriteLine($"Saldo Actual:      ${reader["saldo"]}");
                            Console.WriteLine($"Estado:            {reader["estado"]}");
                        }
                        else
                        {
                            Console.WriteLine($"\nNo se encontró ninguna tarjeta con el número de cuenta: {cuenta}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError al consultar el detalle: " + ex.Message);
                }
            }
        }

        static bool DarDeBajaTarjeta(int cuenta)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "DELETE FROM tarjetas WHERE num_cuenta = @cuenta";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cuenta", cuenta);

                try
                {
                    conn.Open();
                   
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    // Si es mayor a 0 (o sea, 1), devuelve true. Si no, false.
                    return filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError en la base de datos al dar de baja: " + ex.Message);
                    return false;
                }
            }
        }
    }
}