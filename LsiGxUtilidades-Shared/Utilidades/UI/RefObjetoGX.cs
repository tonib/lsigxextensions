using System;
using System.Collections.Generic;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using System.Windows.Forms;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Utilidades.UI
{
    /// <summary>
    /// Informacion sobre un objeto de genexus a mostrar en un grid.
    /// </summary>
    public class RefObjetoGX
    {

        /// <summary>
        /// Identificador del objeto genexus al que hace referencia.
        /// Nulo si no hay objeto al que referenciar.
        /// </summary>
        public Guid IdObjetoGX { get; set; }

        /// <summary>
        /// Object entity key. Equals to EntityKey.Empty if the reference is empty
        /// </summary>
        public EntityKey Key { get; set; }

        /// <summary>
        /// Nombre el objeto genexus
        /// </summary>
        public string NombreObjeto { get; set; }

        /// <summary>
        /// Descripcion del objeto genexus
        /// </summary>
        public string DescripcionObjeto { get; set; }

        /// <summary>
        /// Nombre del tipo del objeto genexus (atributo, procedimiento, etc)
        /// </summary>
        public string TipoObjeto { get; set; }

        /// <summary>
        /// Cierto si la referencia es un falso positivo.
        /// Aparece de un color distinto en el grid.
        /// </summary>
        public bool PosibleFalsoPositivo { get; set; }

        /// <summary>
        /// Folder name that contains the object
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Object's last modification date
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Object's last modification user 
        /// </summary>
        public string LastUser { get; set; }

        /// <summary>
        /// Get the referenced object on the design model
        /// Null if the object was not found
        /// </summary>
        public KBObject ObjetoGX
        {
            get { return GetModelObject(UIServices.KB.CurrentModel); }
        }

        /// <summary>
        /// Get the referenced object on a given model
        /// </summary>
        /// <param name="model">Model where to get the object</param>
        /// <returns>The object at the model. Null if it was not found</returns>
        public KBObject GetModelObject(KBModel model)
        {
            if (EsReferenciaNula)
                return null;
            return model.Objects.Get(IdObjetoGX);
        }

        /// <summary>
        /// Construir esta instancia a partir de un objeto
        /// </summary>
        /// <param name="objeto"></param>
        protected void Inicializar(KBObject objeto) {

            if (objeto == null)
            {
                // Es una referencia nula
                NombreObjeto = DescripcionObjeto = TipoObjeto = Folder = LastUser = string.Empty;
                Key = EntityKey.Empty;
            }
            else
            {
                // Es una referencia no nula
                IdObjetoGX = objeto.Guid;
                Key = objeto.Key;
                NombreObjeto = objeto.Name;
                DescripcionObjeto = objeto.Description;
                TipoObjeto = objeto.TypeDescriptor.Name;
                LastUpdate = objeto.LastUpdate;
                LastUser = objeto.User != null ? objeto.User.Name : string.Empty;

                // Show folder as qualified name. Ignore the root module prefix
                Folder = objeto.Parent != null ? objeto.Parent.QualifiedName.ToString() : string.Empty;

            }
        }

        /// <summary>
        /// Intenta construir una referencia a un objeto genexus a partir de su nombre.
        /// Si no se encuentra un objeto con dicho nombre, se construye una referencia nula.
        /// </summary>
        /// <param name="nombreObjeto">Nombre del objeto a que referenciar</param>
        protected void Inicializar(string nombreObjeto)
        {
            KBObject o = KBaseGX.GetCallableObject(nombreObjeto);
            Inicializar(o);
        }

        /// <summary>
        /// Constructor de una referencia nula.
        /// </summary>
        protected RefObjetoGX() {
            Inicializar(null as KBObject);
        }

        /// <summary>
        /// Constructor de una referencia a un objeto genexus.
        /// </summary>
        /// <param name="objeto">Objeto al que referenciar. Puede ser nulo</param>
        public RefObjetoGX(KBObject objeto)
        {
            Inicializar(objeto);
        }

        /// <summary>
        /// Devuelve cierto si es una referencia nula a un objeto (sin referencia)
        /// </summary>
        public bool EsReferenciaNula
        {
            get { return IdObjetoGX == null || IdObjetoGX == Guid.Empty; }
        }

        /// <summary>
        /// Intenta construir una referencia a un objeto genexus a partir de su nombre.
        /// Si no se encuentra un objeto con dicho nombre, se construye una referencia nula.
        /// </summary>
        /// <param name="nombreObjeto">Nombre del objeto a que referenciar</param>
        public RefObjetoGX(string nombreObjeto)
        {
            Inicializar( nombreObjeto );
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(NombreObjeto))
                return "Null reference";
            else
                return NombreObjeto + " - " + DescripcionObjeto;
        }

        /// <summary>
        /// Abre el objeto genexus referenciado en el editor
        /// </summary>
        virtual public void AbrirReferencia()
        {
            try
            {
                if (!EsReferenciaNula) {
                    // Abrir el objeto genexus:
                    KBObject objetoGx = ObjetoGX;
                    /*if (objetoGx == null)
                        throw new Exception("No object found with id = " + IdObjetoGX.ToString());*/
                    if (objetoGx == null)
                    {
                        MessageBox.Show("No object found with id = " + IdObjetoGX.ToString());
                        return;
                    }
                    UIServices.Objects.Open(objetoGx, OpenDocumentOptions.CurrentVersion);
                }
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        public override bool Equals(object obj)
        {
            RefObjetoGX r = obj as RefObjetoGX;
            if (r == null)
                return false;

            return r.IdObjetoGX == IdObjetoGX;
        }

        public override int GetHashCode()
        {
            return IdObjetoGX.GetHashCode();
        }
    }
}
