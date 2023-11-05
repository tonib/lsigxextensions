using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Utilidades.Threading
{
    /// <summary>
    /// Implementacion de un patron productor / consumidor asincrono
    /// TODO: En .NET 4.0, este patron esta soportado por una clase estandar. Cuando se 
    /// TODO: pueda usar, quitarlo
    /// Plagiado de http://stackoverflow.com/a/1656662
    /// </summary>
    public class ColaTareas<T> : IDisposable where T : class, IExecutable
    {
        /// <summary>
        /// Objeto para bloquear la ejecucion de los threads consumidores cuando la cola
        /// esta vacia
        /// </summary>
        private object SemaforoCola = new object();

        /// <summary>
        /// Threads consumidores
        /// </summary>
        private Thread[] ThreadsConsumidores;

        /// <summary>
        /// La cola de tareas pendientes
        /// </summary>
        private Queue<T> ColaTareasPendientes = new Queue<T>();

        /// <summary>
        /// Nº maximo de tareas que puede contener la cola. Si es cero, la cola no tiene limite
        /// </summary>
        private int TamMaximoCola;

        /// <summary>
        /// This instance has been disposed?
        /// </summary>
        public bool Disposed { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="workerCount">Numero de threads a crear para consumir las tareas</param>
        /// <param name="tamMaximoCola">Tamaño maximo de tareas pendiente que puede contener la 
        /// cola. Si es cero, la cola no tiene limite</param>
        public ColaTareas(int workerCount, int tamMaximoCola)
        {
            TamMaximoCola = tamMaximoCola;
            ThreadsConsumidores = new Thread[workerCount];

            // Create and start a separate thread for each worker
            for (int i = 0; i < workerCount; i++)
            {
                ThreadsConsumidores[i] = new Thread(Consume);
                // Esto es para que se mate automaticamente los threads cuando se cierra la aplicacion
                ThreadsConsumidores[i].IsBackground = true; 
                ThreadsConsumidores[i].Start();
            }
        }

        /// <summary>
        /// Finaliza los threads
        /// </summary>
        public void Dispose()
        {
            // Clear pending tasks
            Disposed = true;
            VaciarCola();

            // Enqueue one null task per worker to make each exit.
            foreach (Thread worker in ThreadsConsumidores)
                NuevaTarea(null);
            // Esperar a que los threads finalicen
            foreach (Thread worker in ThreadsConsumidores) 
                worker.Join();
        }

        /// <summary>
        /// Vacia la cola de tareas pendientes
        /// </summary>
        public void VaciarCola()
        {
            lock (SemaforoCola)
            {
                ColaTareasPendientes.Clear();
            }
        }

        /// <summary>
        /// Agrega una nueva tarea a la cola de tareas
        /// </summary>
        /// <param name="task">Task to execute. If null, the worker thread that executes the task will end</param>
        /// <returns>Cierto si se ha aceptado la tarea en la cola. Puede recharzarse si se ha llegado
        /// al tamaño maximo de cola</returns>
        public bool NuevaTarea(T task)
        {
            lock (SemaforoCola)
            {
                if (Disposed && task != null)
                    return false;

                if (TamMaximoCola > 0 && ColaTareasPendientes.Count >= TamMaximoCola)
                    return false;
                ColaTareasPendientes.Enqueue(task);
                Monitor.PulseAll(SemaforoCola);
                return true;
            }
        }

        /// <summary>
        /// Funcion ejecutada por cada uno de los threads consumidores
        /// </summary>
        private void Consume()
        {
            while (true)
            {
                // Obtener la proxima tarea a ejecutar. Si no hay, esperar
                T task;
                lock (SemaforoCola)
                {
                    while (ColaTareasPendientes.Count == 0) 
                        Monitor.Wait(SemaforoCola);
                    task = ColaTareasPendientes.Dequeue();
                }

                if (task == null) 
                    // Terminar el thread
                    return;

                // Ejecutar la tarea
                try
                {
                    task.Execute();
                }
                catch(Exception ex) 
                {
                    Log.ShowException(ex);
                }
            }
        }
    }
}
