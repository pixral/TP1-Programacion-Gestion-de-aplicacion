<?php
// 1. Iniciamos sesión para poder leer el "casillero" de memoria
session_start();

if (!isset($_SESSION['usuario_dni'])) {
    header("Location: ingreso.html");
    exit();
}

$host = 'localhost';
$db   = 'mi_banco_db';
$user = 'root';
$pass = '';
$charset = 'utf8mb4';

$dsn = "mysql:host=$host;dbname=$db;charset=$charset";

try {
    $pdo = new PDO($dsn, $user, $pass);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    
    $dni_cliente = $_SESSION['usuario_dni'];

    // PASO 2: traemos los datos de la tarjeta del cliente logueado
    $sql_tarjeta = "SELECT numero_tarjeta, banco_emisor, saldo FROM tarjetas WHERE dni_titular = :dni";
    $stmt_t = $pdo->prepare($sql_tarjeta);
    $stmt_t->execute([':dni' => $dni_cliente]);
    $tarjeta = $stmt_t->fetch();

    // PASO 3: traemos el historial de liquidaciones haciendo un JOIN con las tarjetas
    $sql_liq = "SELECT l.periodo, l.fecha_vencimiento, l.total_a_pagar, l.pago_minimo 
                FROM liquidaciones l
                INNER JOIN tarjetas t ON l.num_cuenta = t.num_cuenta
                WHERE t.dni_titular = :dni
                ORDER BY l.periodo DESC"; // Muestra la última liquidación primero
    
    $stmt_l = $pdo->prepare($sql_liq);
    $stmt_l->execute([':dni' => $dni_cliente]);
    $liquidaciones = $stmt_l->fetchAll(); // trae toda las filas

} catch (PDOException $e) {
    die("Error: al cargar los datos: " . $e->getMessage());
}
?>

<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <title>Mis Tarjetas - Progra3Card</title>
    <script src="https://cdn.jsdelivr.net/npm/@tailwindcss/browser@4"></script>
</head>
<body class="bg-gray-100 font-sans">

    <div class="container mx-auto p-6 max-w-4xl">
        <div class="flex justify-between items-center bg-white p-6 rounded-lg shadow-md mb-6">
            <div>
                <h1 class="text-2xl font-bold text-gray-800">Bienvenido, <?php echo htmlspecialchars($_SESSION['usuario_nombre']); ?> 👋</h1>
                <p class="text-sm text-gray-500">DNI: <?php echo htmlspecialchars($dni_cliente); ?></p>
            </div>
            <a href="salir.php" class="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded shadow transition">Cerrar Sesión</a>
        </div>

        <?php if ($tarjeta): ?>
            <div class="bg-gradient-to-r from-blue-600 to-indigo-700 text-white p-6 rounded-xl shadow-lg mb-6">
                <div class="flex justify-between items-start">
                    <div>
                        <p class="text-xs uppercase tracking-wider opacity-70">Banco Emisor</p>
                        <h2 class="text-xl font-bold mb-4"><?php echo htmlspecialchars($tarjeta['banco_emisor']); ?></h2>
                    </div>
                    <span class="text-xs bg-white/20 px-2 py-1 rounded">Progra3card</span>
                </div>
                <div class="mb-4">
                    <p class="text-xs opacity-70">Número de Tarjeta</p>
                    <p class="text-xl tracking-widest font-mono">•••• •••• •••• <?php echo substr($tarjeta['numero_tarjeta'], -4); ?></p>
                </div>
                <div>
                    <p class="text-xs opacity-70">Saldo Actual en la Cuenta</p>
                    <p class="text-2xl font-bold">$<?php echo number_format($tarjeta['saldo'], 2, ',', '.'); ?></p>
                </div>
            </div>
        <?php else: ?>
            <div class="bg-yellow-100 border-l-4 border-yellow-500 text-yellow-700 p-4 mb-6" role="alert">
                <p>No se encontraron tarjetas activas asociadas a este usuario.</p>
            </div>
        <?php endif; ?>

        <div class="bg-white p-6 rounded-lg shadow-md">
            <h3 class="text-lg font-bold text-gray-800 mb-4">Historial de Liquidaciones Mensuales</h3>
            
            <?php if (count($liquidaciones) > 0): ?>
                <div class="overflow-x-auto">
                    <table class="w-full text-left border-collapse">
                        <thead>
                            <tr class="bg-gray-200 text-gray-700 uppercase text-xs">
                                <th class="p-3">Periodo</th>
                                <th class="p-3">Vencimiento</th>
                                <th class="p-3">Pago Mínimo</th>
                                <th class="p-3">Total a Pagar</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-200 text-sm">
                            <?php foreach ($liquidaciones as $liq): ?>
                                <tr class="hover:bg-gray-50">
                                    <td class="p-3 font-semibold text-blue-600"><?php echo htmlspecialchars($liq['periodo']); ?></td>
                                    <td class="p-3"><?php echo htmlspecialchars($liq['fecha_vencimiento']); ?></td>
                                    <td class="p-3">$<?php echo number_format($liq['pago_minimo'], 2, ',', '.'); ?></td>
                                    <td class="p-3 font-bold text-gray-900">$<?php echo number_format($liq['total_a_pagar'], 2, ',', '.'); ?></td>
                                </tr>
                            <?php endbox; ?>
                        </tbody>
                    </table>
                </div>
            <?php else: ?>
                <p class="text-gray-500 text-sm italic">No registra liquidaciones emitidas hasta el momento.</p>
            <?php endif; ?>
        </div>
    </div>

</body>
</html>