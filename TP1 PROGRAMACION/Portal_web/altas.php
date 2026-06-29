<?php
// 1. Configuración de la conexión a la base de datos
$host = 'localhost';
$db   = 'mi_banco_db';
$user = 'root';
$pass = ''; 
$charset = 'utf8mb4';

$dsn = "mysql:host=$host;dbname=$db;charset=$charset";

try {
    $pdo = new PDO($dsn, $user, $pass);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
} catch (PDOException $e) {
    die("Error de conexión: " . $e->getMessage());
}

// 2. Verificar que los datos lleguen por el método POST
if ($_SERVER["REQUEST_METHOD"] == "POST") {
    // Capturamos los datos del formulario de registro
    $dni = $_POST['dni'] ?? '';
    $nuevo_usuario = $_POST['usuario'] ?? '';
    $nuevo_password = $_POST['password'] ?? '';

    if (empty($dni) || empty($nuevo_usuario) || empty($nuevo_password)) {
        die("Error: complete todos los campos.");
    }

    try {
        // PASO 1: Verificar si el DNI ya tiene una tarjeta cargada (por C#)
        $sql_tarjeta = "SELECT num_cuenta FROM tarjetas WHERE dni_titular = :dni";
        $stmt = $pdo->prepare($sql_tarjeta);
        $stmt->execute([':dni' => $dni]);
        
        // fetch() nos da la fila si existe, o false si no existe
        $tarjeta = $stmt->fetch();

        if (!$tarjeta) {
            // Si no hay tarjeta asignada a ese DNI, rebotamos el registro por seguridad
            echo "<h2>Error: Su DNI no figura con una tarjeta emitida por el banco. Contacte a administración.</h2>";
            echo "<a href='registro.html'>Volver a intentar</a>";
        } else {
            // PASO 2: Si tiene tarjeta, actualizamos sus credenciales web (el UPDATE)
            // Guardamos en texto plano como pide la simplificación del TP
            $sql_update = "UPDATE usuarios 
                           SET usuario = :usuario, password = :password 
                           WHERE documento = :dni";
            
            $stmt_update = $pdo->prepare($sql_update);
            $stmt_update->execute([
                ':usuario'  => $nuevo_usuario,
                ':password' => $nuevo_password,
                ':dni'      => $dni
            ]);

            echo "<h2>Cuenta activada.</h2>";
            echo "<p>Ya podés iniciar sesión para ver tus liquidaciones.</p>";
            echo "<a href='ingreso.html'>Ir al Login</a>";
        }

    } catch (PDOException $e) {
        echo "Error: en el proceso de registro: " . $e->getMessage();
    }
} else {
    // Si intentan entrar a altas.php directo por la URL sin pasar por el formulario
    header("Location: registro.html");
    exit();
}
?>