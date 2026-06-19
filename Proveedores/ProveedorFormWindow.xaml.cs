using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POS.Proveedores
{
    /// <summary>
    /// Lógica de interacción para ProveedorFormWindow.xaml
    /// </summary>
    public partial class ProveedorFormWindow : Window
    {
        public TProveedores Proveedor { get; private set; }

        public ProveedorFormWindow()
        {
            InitializeComponent();
            Proveedor = new TProveedores();
        }

        public ProveedorFormWindow(TProveedores proveedorExistente) : this()
        {
            Proveedor = proveedorExistente;
            txtNombre.Text = proveedorExistente.cNombreP;
            txtContacto.Text = proveedorExistente.cContacto;
            txtTelefono.Text = proveedorExistente.cTelefono;
            txtEmail.Text = proveedorExistente.cEmail;
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text)|| string.IsNullOrWhiteSpace(txtContacto.Text) || string.IsNullOrWhiteSpace(txtTelefono.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Porfavor llena todos los datos");
                return;
            }

            Proveedor.cNombreP = txtNombre.Text;
            Proveedor.cContacto = txtContacto.Text;
            Proveedor.cTelefono = txtTelefono.Text;
            Proveedor.cEmail = txtEmail.Text;

            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
