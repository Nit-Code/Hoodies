```
  _   _                 _ _               ____                  
 | | | | ___   ___   __| (_) ___  ___    |  _ \ ___ _ __   ___  
 | |_| |/ _ \ / _ \ / _` | |/ _ \/ __|   | |_) / _ \ '_ \ / _ \ 
 |  _  | (_) | (_) | (_| | |  __/\__ \   |  _ <  __/ |_) | (_) |
 |_| |_|\___/ \___/ \__,_|_|\___||___/___|_| \_\___| .__/ \___/ 
                                    |_____|        |_|          
```									
## Para obtener ejecutables 
1. [Acceder al archivo de este repositorio FinalDistribution/FinalDistribution.zip](https://github.com/SpaceGauchoDev/Hoodies/blob/main/FinalDistribution/FinalDistribution.zip)
2. Descargar el archivo con el botón de "Download" o "Descarga", en la parte central derecha de la pantalla.
3. Descomprimir el fichero 

## Sesiones de prueba posibles
Al ser este un juego competitivo multijugador online, hemos desarrollado dos métodos de validación que funcionan bajo diferentes contextos.

### Sesión de validación local
Dos clientes y un servidor serán lanzados en su computadora. 
#### Registro
Para acceder al juego debe registrarse con un email valido, verificar la cuenta y finalmente acceder. 
Debe hacer esto por cada cliente, es decir un mínimo de dos registros para una partida, pero solo una vez. 
Si ya lo hizo en una session de validación anterior puede usar las credenciales ya generadas.

Este tipo de ejecución fue muy usado durante el desarrollo ya que permite debugear el flujo completo de juego, exceptuando la interacción con la API de AWS GameLift.

Para iniciar una session de validación local, simplemente hay que ejecutar LocalTesting.bat del fichero FinalDistribution.

### Sesión de validación remota
Dos clientes serán lanzados en su computadora y se conectaran con un servidor remoto. 
#### Registro
Para acceder al juego debe registrarse con un email valido, verificar la cuenta y finalmente . 
Debe hacer esto por cada cliente, es decir un mínimo de dos registros para una partida, pero solo una vez. 
Si ya lo hizo en una session de validación anterior puede usar las credenciales ya generadas.

Este tipo de ejecución es mas intrincado pero permite evaluar el uso completo del entorno de producción real.

Para iniciar una session de validación remota, simplemente hay que ejecutar RemoteTesting.bat del fichero FinalDistribution.

## Para preparar un ambiente de desarrollo
- [Instalar comandos de consola de git para escritorio](https://git-scm.com/downloads)
	1. Click derecho en la carpeta donde queremos descargar el repositorio
	2. Seleccionar Git Bash Here del menu contextual
	3. En la ventana emergente ingresar el siguiente commando sin comillas "git clone https://github.com/SpaceGauchoDev/Hoodies.git"
- [Instalar Visual Studio Community 2022](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&channel=Release&version=VS2022&source=VSLandingPage&cid=2030&passive=false)
	- Componentes:
		- .NET desktop development
		- Game development with Unity
- [Instalar AWS Toolkit para Visual Studio 2022](https://marketplace.visualstudio.com/items?itemName=AmazonWebServices.AWSToolkitforVisualStudio2022)
- [Instalar Unity Hub](https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.exe)
	- Descargar version:
		- 2021.3.0f1
	- Agregar módulos: 
		- Linux Build Support (IL2CPP)
		- Linux Dedicated Server Build Support
		- WebGL Build Support
		- Windows Dedicated Server Build Support
- Solicitar acceso a cuenta AWS del equipo al siguiente email: manu1502@gmail.com
- [Instalar AWS Command Line Interface](https://aws.amazon.com/cli/)			
- Solicitar acceso a la carpeta de google drive del equipo al siguiente email: manu1502@gmail.com
- [Instalar Plastic SCM](https://www.plasticscm.com/)
	- Solicitar acceso al workspace al siguiente email: manu1502@gmail.com