using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Packages;
using Artech.Architecture.UI.Framework.Services;
using Artech.FrameworkDE;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Utilidades.UI
{

    /// <summary>
    /// Control grid para mostrar una lista de objetos de la kbase
    /// Para que funcione, el datasource debe ser de tipo ListaOrdenable conteniendo
    /// objeto de tipo RefObjetoGX
    /// </summary>
    public class GridObjetos : DataGridView
    {

        /// <summary>
        /// Column 'name' name
        /// </summary>
        public const string COL_NAME = "ColNombre";

        /// <summary>
        /// Column 'User' name
        /// </summary>
        public const string COL_USER = "ColLastUser";

        /// <summary>
        /// Column 'Updated' name
        /// </summary>
        public const string COL_LASTUPDATE = "ColLastUpdate";

        /// <summary>
        /// Last keys typed on the grid
        /// </summary>
        private StringBuilder SelectionTypingCache = new StringBuilder();

        /// <summary>
        /// Should the grid open selected objects with double click / enter pressed?
        /// </summary>
        public bool OpenObjectsEnabled = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public GridObjetos()
        {
            SetGridDefaultConfiguration(this);

            // Eventos:
            CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.Grid_CellMouseDoubleClick);
            KeyDown += new System.Windows.Forms.KeyEventHandler(this.Grid_KeyDown);
            SelectionChanged += new EventHandler(GridObjetos_SelectionChanged);
            MouseDown += new MouseEventHandler(GridObjetos_MouseDown);
        }

        /// <summary>
        /// Set the standard configuracion for a grid
        /// </summary>
        /// <param name="grid">Grid to configure</param>
        static public void SetGridDefaultConfiguration(DataGridView grid)
        {
            grid.AutoGenerateColumns = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            // Enable clipboard (sure?):
            grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
        }

        /// <summary>
        /// Handle mouse right button click over the grid
        /// </summary>
        void GridObjetos_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point screenPoint = PointToScreen(e.Location);
                ShowContextualMenu(screenPoint);
            }
        }

        /// <summary>
        /// Show contextual menu
        /// </summary>
        /// <param name="clickPoint">Clicked point</param>
        private void ShowContextualMenu(Point clickPoint)
        {
            try
            {
                KBObject selection = ObjetoSeleccionado;
                if (selection == null)
                    return;

                // Be sure the grid is focused, to update command states
                if (!Focused)
                    Focus();

                // Display the contextual menu
                Guid guid = UIPackageGuid.Core;
                string menuKey = "WWObjectsContext";
                UIServices.Menu.ShowContextMenu(guid, menuKey, clickPoint);

            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        DataGridViewColumn GetColumnByName(string columnName)
        {
            return Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(x => x.Name == columnName);
        }

        /// <summary>
        /// Order the grid by object name
        /// </summary>
        public void OrderByObjectName()
        {
            DataGridViewColumn col = GetColumnByName(COL_NAME);
            if (col == null)
                return;
            Sort(col, System.ComponentModel.ListSortDirection.Ascending);
        }

        public void HideColumn(string columnName)
        {
            DataGridViewColumn col = GetColumnByName(columnName);
            if (col == null)
                return;
            col.Visible = false;
        }

        public void CrearColumnasEstandar()
        {

            // Columna nombre
            DataGridViewTextBoxColumn colNombre = new DataGridViewTextBoxColumn();
            colNombre.DataPropertyName = "NombreObjeto";
            colNombre.HeaderText = "Name";
            colNombre.Name = COL_NAME;
            colNombre.ReadOnly = true;

            // Columna tipo objeto
            DataGridViewTextBoxColumn colTipo = new DataGridViewTextBoxColumn();
            colTipo.DataPropertyName = "TipoObjeto";
            colTipo.HeaderText = "Obj. type";
            colTipo.Name = "ColTipo";
            colTipo.ReadOnly = true;

            // Columna descripcion
            DataGridViewTextBoxColumn colDescripcion = new DataGridViewTextBoxColumn();
            colDescripcion.DataPropertyName = "DescripcionObjeto";
            colDescripcion.HeaderText = "Description";
            colDescripcion.Name = "ColDescripcion";
            colDescripcion.ReadOnly = true;
            colDescripcion.Width = 300;

            // Folder column
            DataGridViewTextBoxColumn colFolder = new DataGridViewTextBoxColumn();
            colFolder.DataPropertyName = "Folder";
            colFolder.HeaderText = "Folder";
            colFolder.Name = "ColFolder";
            colFolder.ReadOnly = true;

            // Last modificacion column
            DataGridViewTextBoxColumn colLastUpdate = new DataGridViewTextBoxColumn();
            colLastUpdate.DataPropertyName = "LastUpdate";
            colLastUpdate.HeaderText = "Updated";
            colLastUpdate.Name = COL_LASTUPDATE;
            colLastUpdate.ReadOnly = true;

            // Last user column
            DataGridViewTextBoxColumn colLastUser = new DataGridViewTextBoxColumn();
            colLastUser.DataPropertyName = "LastUser";
            colLastUser.HeaderText = "User";
            colLastUser.Name = COL_USER;
            colLastUser.ReadOnly = true;

            Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                colNombre, colTipo, colDescripcion, colFolder, colLastUpdate , colLastUser
            });
        }

        /// <summary>
        /// NO USAR: En su lugar, usar SetObjetos y GetObjetos.
        /// TODO: Cagada que algun dia habria que arreglar... 
        /// No se puede borrar porque hay referencias en los forms que incluyen este control
        /// </summary>
        public object Objetos
        {
            get { return DataSource; }
            set { DataSource = value; }
        }

        /// <summary>
        /// Establece la lista de objetos a mostrar en el grid
        /// </summary>
        public void SetObjetos<T>(IList<T> objetos) where T : RefObjetoGX {
            DataSource = objetos;
            if (objetos is SortableList<T>)
            {
                ((SortableList<T>)objetos).ListChanged += new System.ComponentModel.ListChangedEventHandler(GridObjetos_ListChanged);
            }

        }

        /// <summary>
        /// Get the objects displayed on the grid
        /// </summary>
        /// <typeparam name="T">Type of displayed items</typeparam>
        /// <returns>The list of object references</returns>
        public SortableList<T> GetObjetos<T>() where T : RefObjetoGX
        {
            return (SortableList<T>)DataSource;
        }

        /// <summary>
        /// Get the objects displayed on the grid
        /// </summary>
        /// <returns>The list of object references</returns>
        public System.Collections.IList GetObjectsList()
        {
            return (System.Collections.IList)DataSource;
        }

        /// <summary>
        /// Devuelve el primer objeto genexus seleccionado.
        /// Si no hay ninguno, devuelve null
        /// </summary>
        public KBObject ObjetoSeleccionado
        {
            get
            {
                List<KBObject> seleccion = ObjetosSeleccionados;
                if (seleccion.Count == 0)
                    return null;
                return seleccion[0];
            }
        }

        /// <summary>
        /// Get selected object on grid, from the design model. Null references are ignored
        /// </summary>
        public List<KBObject> ObjetosSeleccionados
        {
            get { return GetSelectedObjects(UIServices.KB.CurrentModel); }
        }

        /// <summary>
        /// Get selected object on grid, from a given model. Null references are ignored
        /// </summary>
        /// <returns>The selected objects</returns>
        public List<KBObject> GetSelectedObjects(KBModel model)
        {
            List<KBObject> objetos = new List<KBObject>();
            foreach (RefObjetoGX referencia in ReferenciasSeleccionadas)
            {
                KBObject o = referencia.GetModelObject(model);
                if (o != null)
                    objetos.Add(o);
            }
            return objetos;
        }

        /// <summary>
        /// Devuelve las referencias a objetos seleccionadas
        /// </summary>
        public List<RefObjetoGX> ReferenciasSeleccionadas
        {
            get
            {
                if (DataSource == null)
                    return new List<RefObjetoGX>();

                List<RefObjetoGX> referencias = new List<RefObjetoGX>();
                System.Collections.IList objetos = (System.Collections.IList)DataSource;

                // Table columns are sortable, and SelectedRows are not returned on the display order
                // So, sort first:
                IEnumerable<DataGridViewRow> sortedRows = SelectedRows
                    .Cast<DataGridViewRow>()
                    .OrderBy(x => x.Index);

                foreach (DataGridViewRow fila in sortedRows)
                {
                    RefObjetoGX r = (RefObjetoGX)objetos[fila.Index];
                    referencias.Add(r);
                }
                return referencias;
            }
        }

        public RefObjetoGX SelectedReference
        {
            get
            {
                List<RefObjetoGX> refs = ReferenciasSeleccionadas;
                if (refs.Count == 0)
                    return null;
                else
                    return refs[0];
            }
        }

        /// <summary>
        /// Abre los archivos / objetos seleccionados
        /// </summary>
        public void AbrirObjetosSeleccionados()
        {
            if (DataSource == null || !OpenObjectsEnabled)
                return;

            int nAbiertos = 0;
            foreach( RefObjetoGX r in ReferenciasSeleccionadas) 
            {
                r.AbrirReferencia();

                // Para no cagarla abriendo chorrocientos objetos:
                nAbiertos++;
                if (nAbiertos > 10)
                    break;
            }
        }

        /// <summary>
        /// Se ha pulsado alguna tecla sobre la lista
        /// </summary>
        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                AbrirObjetosSeleccionados();
            else if (e.KeyCode == Keys.C && e.Control && e.Alt)
            {
                // Forzar un copy de la tabla. Genexus gestiona el Ctrl + C y no copia el texto
                DataObject o = this.GetClipboardContent();
                Clipboard.SetDataObject(o);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Delete && e.Control)
                // Delete selected objects
                DeleteSelectedObjects();
        }

        /// <summary>
        /// Se ha hecho doble click en algun elemento de la lista
        /// </summary>
        private void Grid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex == -1)
                // Se ha hecho doble click sobre la cabecera
                return;

            AbrirObjetosSeleccionados();
        }

        /// <summary>
        /// Busca la tool window a la que pertenece este grid.
        /// </summary>
        /// <returns>La tool window a la que pertenece el control. null si no se encontro</returns>
        private AbstractToolWindow ObtenerToolWindow()
        {
            Control control = this.Parent;
            while( control != null && !(control is AbstractToolWindow) )
                control = control.Parent;
            return (AbstractToolWindow)control;
        }

        /// <summary>
        /// Notify to genexus that the selection has changed
        /// </summary>
        public void UpdateToolWindowSelection()
        {
            AbstractToolWindow tw = ObtenerToolWindow();
            if (tw != null && tw is ToolWindowBase)
                ((ToolWindowBase)tw).SeleccionarObjetos(ObjetosSeleccionados);
        }

        /// <summary>
        /// Llamado cuando cambia la seleccion de los objetos en el grid.
        /// Informa de la seleccion a genexus.
        /// </summary>
        private void GridObjetos_SelectionChanged(object sender, EventArgs e)
        {
            UpdateToolWindowSelection();
        }

        // This can be removed???
        private void InitializeComponent()
        {
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        /// <summary>
        /// Llamado cuando se cambia la lista de referencias a objetos
        /// Cambia el color de fondo de las celdas segun sea un posible falso positivo o no.
        /// </summary>
        private void GridObjetos_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            System.Collections.IList objetos = (System.Collections.IList)DataSource;
            for (int i = 0; i < objetos.Count; i++)
            {
                RefObjetoGX r = (RefObjetoGX)objetos[i];
                if (r.PosibleFalsoPositivo)
                    Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                else
                    Rows[i].DefaultCellStyle.BackColor = Color.White;
            }
        }

        /// <summary>
        /// Removes any row referencing an object
        /// </summary>
        /// <param name="objectId">Object id which remove references</param>
        public void RemoveReferencesToObject(Guid objectId)
        {
            System.Collections.IList gridObjects = (System.Collections.IList)DataSource;
            List<RefObjetoGX> refsToRemove = new List<RefObjetoGX>();
            foreach (RefObjetoGX r in gridObjects)
            {
                if (r.IdObjetoGX == objectId)
                    refsToRemove.Add(r);
            }
            refsToRemove.ForEach(x => gridObjects.Remove(x));
        }

        /// <summary>
        /// Removes any row referencing an object
        /// </summary>
        /// <param name="o">Object which remove references</param>
        public void RemoveReferencesToObject(KBObject o)
        {
            RemoveReferencesToObject(o.Guid);
        }

        /// <summary>
        /// Deletes selected objects from kbase
        /// </summary>
        public void DeleteSelectedObjects()
        {
            try
            {
                List<RefObjetoGX> selection = ReferenciasSeleccionadas;
                if (selection.Count == 0)
                    return;
                
                // Delete objects:
                List<KBObject> selectedObjects = ObjetosSeleccionados;
                try
                {
                    if (UIServices.Objects.Delete(selectedObjects, true))
                        // All objects removed
                        selectedObjects.ForEach(x => RemoveReferencesToObject(x));
                    else
                    {
                        // Check objects really deleted:
                        foreach (RefObjetoGX r in selection)
                        {
                            if (r.ObjetoGX == null)
                                RemoveReferencesToObject(r.IdObjetoGX);
                        }
                    }
                }
                
                catch (Exception ex)
                {
                    Log.ShowException(ex);
                }
            }
            catch (Exception ex2)
            {
                Log.ShowException(ex2);
            }
        }

        /// <summary>
        /// Reset selection typing cache when control receives focus
        /// </summary>
        protected override void OnEnter(EventArgs e)
        {
            SelectionTypingCache.Length = 0;
            base.OnEnter(e);
        }

        /// <summary>
        /// Reset selection typing cache when mouse is clicked
        /// </summary>
        protected override void OnClick(EventArgs e)
        {
            SelectionTypingCache.Length = 0;
            base.OnClick(e);
        }

        /// <summary>
        /// It does object selection with the keyboard
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            char key = (char)e.KeyValue;
            if (Char.IsLetterOrDigit(key) && e.Modifiers == 0)
            {
                SelectionTypingCache.Append(Char.ToLower(key));
                SelectFirstObjectByNameStart(SelectionTypingCache.ToString());
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else
            {
                SelectionTypingCache.Length = 0;
                base.OnKeyDown(e);
            }

        }

        /// <summary>
        /// Selects the first object with a name starting with some text
        /// </summary>
        /// <param name="selectionText">Start text of the object name to select</param>
        public void SelectFirstObjectByNameStart(string selectionText)
        {
            System.Collections.IList items = (System.Collections.IList)DataSource;
            
            foreach (DataGridViewRow row in Rows)
            {
                RefObjetoGX r = row.DataBoundItem as RefObjetoGX;
                if (r == null)
                    continue;

                if (r.NombreObjeto.ToLower().StartsWith(selectionText))
                {
                    if (!SelectedRows.Contains(row))
                    {
                        // This will throw an exception if the first row is hidden:
                        //CurrentCell = row.Cells[0];
                        DataGridViewCell cell = row.Cells.Cast<DataGridViewCell>()
                            .FirstOrDefault(x => x.Visible);
                        if (cell != null)
                            CurrentCell = cell;
                    }
                    break;
                }
            }

        }

    }
}
