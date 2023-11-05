using Artech.Common.Framework.Commands;

namespace LSI.Packages.Extensiones
{
    /// <summary>
    /// Gestion de los comandos de la extension (creo)
    /// Aqui se declaran cada uno de los comandos que puede lanzar la extension.
    /// </summary>
    public class CommandKeys
    {
 
        // TODO: Esta clase no es necesaria: Crear los command keys en el CommandManager directamente

        /// <summary>
        /// Comando para verificar el obejto actual
        /// </summary>
        private static CommandKey _VerificarObjeto = new CommandKey(Package.Guid, "VerificarObjeto");
        /// <summary>
        /// Comando para verificar el obejto actual
        /// </summary>
        public static CommandKey VerificarObjeto
        {
            get { return _VerificarObjeto; }
        }

        /// <summary>
        /// Comando para crear un procedimiento para obtener el valor de un atributo
        /// </summary>
        private static CommandKey _CrearProcObtenerValor = new CommandKey(Package.Guid, "CrearProcObtenerValor");
        /// <summary>
        /// Comando para crear un procedimiento para obtener el valor de un atributo
        /// </summary>
        public static CommandKey CrearProcObtenerValor
        {
            get { return _CrearProcObtenerValor; }
        }

        /// <summary>
        /// Comando para crear un procedimiento para borrar un registro
        /// </summary>
        private static CommandKey _CrearProcBorradoRegistro = new CommandKey(Package.Guid, "CrearProcBorradoRegistro");
        /// <summary>
        /// Comando para crear un procedimiento para para borrar un registro
        /// </summary>
        public static CommandKey CrearProcBorradoRegistro
        {
            get { return _CrearProcBorradoRegistro; }
        }

        /// <summary>
        /// Comando para crear insertar en la posicion actual del editor un for each de una tabla
        /// </summary>
        private static CommandKey _InsertarForEach = new CommandKey(Package.Guid, "InsertarForEach");
        /// <summary>
        /// Comando para crear insertar en la posicion actual del editor un for each de una tabla
        /// </summary>
        public static CommandKey InsertarForEach
        {
            get { return _InsertarForEach; }
        }

        /// <summary>
        /// Comando para crear insertar en el codigo un new de una tabla
        /// </summary>
        private static CommandKey _InsertarNew = new CommandKey(Package.Guid, "InsertarNew");
        /// <summary>
        /// Comando para crear insertar en el codigo un new de una tabla
        /// </summary>
        public static CommandKey InsertarNew
        {
            get { return _InsertarNew; }
        }

        /// <summary>
        /// Comando para crear un sdt con la misma estructura que una tabla
        /// </summary>
        private static CommandKey _CrearSdtDeTabla = new CommandKey(Package.Guid, "CrearSdtDeTabla");
        /// <summary>
        /// Comando para crear un sdt con la misma estructura que una tabla
        /// </summary>
        public static CommandKey CrearSdtDeTabla
        {
            get { return _CrearSdtDeTabla; }
        }
        
        /// <summary>
        /// Comando para cargar un sdt con la estructura de una tabla
        /// </summary>
        private static CommandKey _CrearProcCargaSdt = new CommandKey(Package.Guid, "CrearProcCargaSdt");
        /// <summary>
        /// Comando para crear un sdt con la misma estructura que una tabla
        /// </summary>
        public static CommandKey CrearProcCargaSdt
        {
            get { return _CrearProcCargaSdt; }
        }

        /// <summary>
        /// Comando para actualizar un registro de la bbdd con los valores de un sdt
        /// </summary>
        private static CommandKey _CrearProcActualizacionSdt = new CommandKey(Package.Guid, "CrearProcActualizacionSdt");
        /// <summary>
        /// Comando para actualizar un registro de la bbdd con los valores de un sdt
        /// </summary>
        public static CommandKey CrearProcActualizacionSdt
        {
            get { return _CrearProcActualizacionSdt; }
        }

        /// <summary>
        /// Comando para crear un procedimiento que inserta un registro con los valores de un sdt
        /// </summary>
        private static CommandKey _CrearProcInsercionSdt = new CommandKey(Package.Guid, "CrearProcInsercionSdt");
        /// <summary>
        /// Comando para crear un procedimiento que inserta un registro con los valores de un sdt
        /// </summary>
        public static CommandKey CrearProcInsercionSdt
        {
            get { return _CrearProcInsercionSdt; }
        }

        /// <summary>
        /// Abre la ventana de configuracion de la extension
        /// </summary>
        private static CommandKey _EditarConfiguracion = new CommandKey(Package.Guid, "EditarConfiguracion");
        /// <summary>
        /// Abre la ventana de configuracion de la extension
        /// </summary>
        public static CommandKey EditarConfiguracion
        {
            get { return _EditarConfiguracion; }
        }

        /// <summary>
        /// Crea un procedimiento de insercion de un registro
        /// </summary>
        private static CommandKey _CrearProcInsercion = new CommandKey(Package.Guid, "CrearProcInsercion");
        /// <summary>
        /// Crea un procedimiento de insercion de un registro
        /// </summary>
        public static CommandKey CrearProcInsercion
        {
            get { return _CrearProcInsercion; }
        }

        /// <summary>
        /// Crea un procedimiento de asignacion de un atributo
        /// </summary>
        private static CommandKey _CrearProcAsignarAtributo = new CommandKey(Package.Guid, "CrearProcAsignarAtributo");
        /// <summary>
        /// Crea un procedimiento de asignacion de un atributo
        /// </summary>
        public static CommandKey CrearProcAsignarAtributo
        {
            get { return _CrearProcAsignarAtributo; }
        }

        /// <summary>
        /// Arreglar problemas de un objeto
        /// </summary>
        private static CommandKey _ArreglarProblemas = new CommandKey(Package.Guid, "ArreglarProblemas");
        /// <summary>
        /// Arreglar problemas de un objeto
        /// </summary>
        public static CommandKey ArreglarProblemas
        {
            get { return _ArreglarProblemas; }
        }
        
        /// <summary>
        /// Establece el tamaño del winform actual en el tamaño maximo
        /// </summary>
        private static CommandKey _TamMaximoWinform = new CommandKey(Package.Guid, "TamMaximoWinform");
        /// <summary>
        /// Establece el tamaño del winform actual en el tamaño maximo
        /// </summary>
        public static CommandKey TamMaximoWinform
        {
            get { return _TamMaximoWinform; }
        }

        /// <summary>
        /// Revisar las fuentes no instaladas en el equipo de los reports
        /// </summary>
        private static CommandKey _RevisarFuentes = new CommandKey(Package.Guid, "RevisarFuentes");
        /// <summary>
        /// Revisar las fuentes no instaladas en el equipo de los reports
        /// </summary>
        public static CommandKey RevisarFuentes
        {
            get { return _RevisarFuentes; }
        }

    }
}
