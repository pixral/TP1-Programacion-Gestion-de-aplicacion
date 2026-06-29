<?php
// ¡OBLIGATORIO! 
// esto es para que php prepare un panel para la sesion
session_start();

// Conexión a la base de datos
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

// Procesar el formulario cuando se envía por POST
if ($_SERVER["REQUEST_METHOD"] == "POST") {
    $txt_usuario = $_POST['usuario'] ?? '';
    $txt_password = $_POST['password'] ?? '';

    if (empty($txt_usuario) || empty($txt_password)) {
        die("Error: complete todos los campos.");
    }

    try {
        // Buscamos al usuario que coincida con las credenciales (texto plano temporal)
        $sql = "SELECT documento, nombre, apellido FROM usuarios WHERE usuario = :user AND password = :pass";
        $stmt = $pdo->prepare($sql);
        $stmt->execute([
            ':user' => $txt_usuario,
            ':pass' => $txt_password
        ]);

        $usuario = $stmt->fetch();

        if ($usuario) {
            // ¡LOGIN CORRECTO! Guardamos los datos en la sesión
            $_SESSION['usuario_dni'] = $usuario['documento'];
            $_SESSION['usuario_nombre'] = $usuario['nombre'] . " " . $usuario['apellido'];

            // Redireccionamos al cliente a su panel de liquidaciones
            header("Location: resumen.php");
            exit();
        } else {
            // LOGIN INCORRECTO
            echo "<h2>Error: Usuario o contraseña incorrectos.</h2>";
            echo "<a href='ingreso.html'>Volver a intentar</a>";
        }

    } catch (PDOException $e) {
        echo "Error: en el login: " . $e->getMessage();
    }
} else {
    header("Location: ingreso.html");
    exit();
}
?>