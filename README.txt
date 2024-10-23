# Prueba Biinteli

## Descripción
Prueba Biinteli es una aplicación diseñada para gestionar vuelos y viajes. Permite realizar la búsqueda de vuelos, gestionar rutas y acceder a información de los viajes a través de una API. El backend está construido en ASP.NET Core y utiliza Entity Framework para interactuar con una base de datos SQL Server.

## Tecnologías Utilizadas
- **Lenguaje**: C#
- **Framework**: ASP.NET Core
- **Base de Datos**: MySQL
- **Herramientas de Desarrollo**: Visual Studio
- **Otras Dependencias**: 
  - Entity Framework Core
  - Microsoft.AspNetCore.Mvc
  - Microsoft.EntityFrameworkCore



## Instalación

Para instalar y ejecutar el proyecto en tu máquina local, sigue estos pasos:

1. **Clonar el repositorio**:
   
   ```bash
   git clone https://github.com/Andolon-M/PruebaBiinteli.git
   cd PruebaBiinteli

2. **Restaurar las dependencias**:
   Asegúrate de tener el SDK de .NET instalado. Luego, en la terminal, ejecuta:
   
   ```bash
   dotnet restore
   ```
   
3. **Configurar la base de datos**:
   - Crea una base de datos en SQL Server.
   - Configura la cadena de conexión en el archivo `appsettings.json`:
   ```json
   "ConnectionStrings": {
       "DefaultConnection": "Server=tu_servidor;Database=tu_base_de_datos;User Id=tu_usuario;Password=tu_contraseña;"
   }
   ```

4. **Migrar la base de datos**:
   Ejecuta las migraciones para crear la base de datos y las tablas necesarias:
   ```bash
   dotnet ef database update
   ```

5. **Ejecutar la aplicación**:
   ```bash
   dotnet run
   ```

   La aplicación estará disponible en `http://localhost:5196`.



## Modelo de Carpetas

```plaintext
PruebaBiinteli/
├── Controllers/                # Controladores de la API
│   ├── FlightsController.cs    # Controlador para gestionar vuelos
│   └── JourneyController.cs    # Controlador para gestionar viajes
├── Data/                       # Contexto de la base de datos
│   └── ApplicationDbContext.cs # Contexto principal de la base de datos
├── Dtos/                       # Data Transfer Objects
│   ├── FlightDto.cs            # DTO para vuelos
├── Models/                     # Modelos de datos
│   ├── Flights.cs              # Modelo de la entidad Flight
│   ├── Journey.cs              # Modelo de la entidad Journey
│   └── Transports.cs           # Modelo de la entidad Transports
├── Migrations/                 # Migraciones de Entity Framework
├── appsettings.json            # Configuración de la aplicación
├── appsettings.Development.json # Configuración para el entorno de desarrollo
└── Program.cs                  # Punto de entrada de la aplicación
```



## APIs

### Flights

#### **GET** `/api/Flights`

Este endpoint permite obtener los vuelos disponibles desde una API externa. Los vuelos se almacenan automáticamente en la base de datos para su posterior uso en el cálculo de rutas de viaje.

- **Método**: `GET`
- **URL**: `http://localhost:5196/api/Flights`
- **Descripción**: 
  - Solicita datos de vuelos desde una API externa.
  - Guarda los vuelos obtenidos en la base de datos.
  - No requiere parámetros.
- **Ejemplo de respuesta**:
  
  ```json
  [
    {
      "flightCarrier": "ABC Airlines",
      "flightNumber": "ABC123",
      "origin": "CGT",
      "destination": "MED",
      "price": 120.50
    }
  ]



### Journey

#### **GET** `/api/Journey/find-flights?origen={origen}&destino={destino}`

Este endpoint permite calcular las rutas de vuelo necesarias para llegar a un destino desde un origen. Primero busca en la base de datos vuelos directos y, si no los encuentra, intenta buscar vuelos con escalas para construir una ruta completa.

- **Método**: `GET`
- **URL**: `http://localhost:5196/api/Journey/find-flights`
- **Parámetros**:
  - `origen` (requerido): Código del aeropuerto de origen.
  - `destino` (requerido): Código del aeropuerto de destino.
- **Descripción**:
  - Busca vuelos directos desde el origen al destino.
  - Si no hay vuelos directos, busca vuelos con escalas.
  - Si encuentra una ruta, la guarda en la base de datos para usos posteriores.
  - Si ya existe una ruta registrada para el origen y destino solicitados, devuelve la ruta existente.
- **Ejemplo de solicitud**:
  ```http
  GET /api/Journey/find-flights?origen=CGT&destino=MED
  ```
- **Ejemplo de respuesta**:
  
  - primera vez que se busque la ruta
  
  ````json
  [
    {
      "origin": "bga",
      "destination": "ctg",
      "totalPrice": 3000,
      "flights": [
        {
          "id": 1,
          "origin": "BGA",
          "destination": "BTA",
          "price": 1000,
          "flightNumber": "8020"
        },
        {
          "id": 2,
          "origin": "BTA",
          "destination": "CTG",
          "price": 2000,
          "flightNumber": "8030"
        }
      ]
    },
    {
      "origin": "bga",
      "destination": "ctg",
      "totalPrice": 2000,
      "flights": [
        {
          "id": 5,
          "origin": "BGA",
          "destination": "MED",
          "price": 1000,
          "flightNumber": "8060"
        },
        {
          "id": 6,
          "origin": "MED",
          "destination": "CTG",
          "price": 1000,
          "flightNumber": "8070"
        }
      ]
    }
  ]
  ````
  
  
  
  - Segunda vez que se busque la ruta
  
  
  ```json
  [
    {
      "id": 5,
      "listFlight": "8020, 8030",
      "origin": "BGA",
      "destination": "CTG",
      "price": 3000
    },
    {
      "id": 6,
      "listFlight": "8060, 8070",
      "origin": "BGA",
      "destination": "CTG",
      "price": 2000
    }
  ]
  ```
  
- **Errores posibles**:
  - `400 Bad Request`: Si faltan los parámetros `origen` o `destino`.
  - `404 Not Found`: Si no se encuentran vuelos disponibles para la ruta solicitada.

