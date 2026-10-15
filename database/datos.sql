USE SistemaInventario;
GO

INSERT INTO Categorias (Nombre, Descripcion) VALUES
(N'Electrónica', N'Dispositivos y accesorios electrónicos de uso diario.'),
(N'Hogar',       N'Artículos para el hogar, cocina y decoración.'),
(N'Alimentos',   N'Productos alimenticios y de la canasta básica.'),
(N'Papelería',   N'Útiles escolares y de oficina.'),
(N'Ferretería',  N'Herramientas y materiales de construcción ligera.');

INSERT INTO Suplidores (NombreEmpresa, NombreContacto, Telefono, Correo, SitioWeb) VALUES
(N'Distribuidora Caribe SRL',  N'Juan Pérez',     '809-555-0101', 'ventas@distcaribe.com.do',  'https://www.distcaribe.com.do'),
(N'Importadora del Este SRL',  N'María Gómez',    '829-555-0102', 'contacto@impdeleste.com.do', 'https://www.impdeleste.com.do'),
(N'Comercial Macorís',         N'Carlos Santos',  '849-555-0103', 'info@comercialmacoris.com',  NULL),
(N'Suministros Quisqueya',     N'Ana Rodríguez',  '809-555-0104', 'pedidos@sumquisqueya.com',   'https://www.sumquisqueya.com'),
(N'Grupo Antillano SA',        N'Luis Martínez',  '829-555-0105', 'compras@grupoantillano.com', NULL);

INSERT INTO Productos (Nombre, PrecioUnitario, CategoriaId, SuplidorId) VALUES
(N'Audífonos Bluetooth',            1250.00, 1, 1),
(N'Cargador USB-C 20W',              450.00, 1, 2),
(N'Bocina Portátil',                2100.00, 1, 1),
(N'Mouse Inalámbrico',               650.00, 1, 3),
(N'Juego de Sábanas Queen',         1800.00, 2, 4),
(N'Licuadora 10 Velocidades',       2450.00, 2, 2),
(N'Set de Ollas 3 Piezas',          3200.00, 2, 4),
(N'Lámpara de Mesa LED',             780.00, 2, 5),
(N'Arroz Selecto 5 lb',              285.00, 3, 3),
(N'Aceite de Soya 1 Galón',          620.50, 3, 3),
(N'Café Molido 1 lb',                350.00, 3, 5),
(N'Habichuelas Rojas 1 lb',           95.75, 3, 3),
(N'Cuaderno Profesional 100 Hojas',   85.00, 4, 4),
(N'Caja de Lápices x12',             120.00, 4, 4),
(N'Resma Papel Carta 500 Hojas',     475.00, 4, 1),
(N'Marcadores Permanentes x4',       160.00, 4, 5),
(N'Martillo de Acero 16 oz',         395.00, 5, 5),
(N'Juego de Destornilladores 8 pzs', 540.00, 5, 2),
(N'Cinta Métrica 5 m',               180.00, 5, 2),
(N'Taladro Inalámbrico 12V',        4850.00, 5, 1);
GO

SELECT (SELECT COUNT(*) FROM Categorias) AS Categorias,
       (SELECT COUNT(*) FROM Suplidores) AS Suplidores,
       (SELECT COUNT(*) FROM Productos)  AS Productos;