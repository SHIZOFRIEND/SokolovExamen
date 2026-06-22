using System;
using System.Linq;
using System.Windows;

namespace SokolovExamen
{
    public partial class AddTovar : Window
    {
        private readonly BDModel _db = new BDModel();
        private readonly Tovar _item;

        public AddTovar(Tovar product)
        {
            InitializeComponent();

            _item = product;

            FillComboboxes();
            FillFields();
        }

        private void FillComboboxes()
        {
            cbCategory.ItemsSource = _db.TovarCategory.ToList();
            cbCategory.DisplayMemberPath = "TovarCategoryName";
            cbCategory.SelectedValuePath = "IDTovarCategory";

            cbManufacturer.ItemsSource = _db.Proizvoditeli.ToList();
            cbManufacturer.DisplayMemberPath = "NameProizvoditelia";
            cbManufacturer.SelectedValuePath = "IDProizvoditelia";

            cbSupplier.ItemsSource = _db.Postavshik.ToList();
            cbSupplier.DisplayMemberPath = "PostavshikName";
            cbSupplier.SelectedValuePath = "IDPostavshik";

            cbUnit.ItemsSource = _db.EdIzmer.ToList();
            cbUnit.DisplayMemberPath = "Edzm";
            cbUnit.SelectedValuePath = "IDEdIzm";
        }

        private void FillFields()
        {
            tbId.Text = _item.IDTovar.ToString();
            tbName.Text = _item.TovarName;
            tbArticle.Text = _item.Article;
            tbDescription.Text = _item.Description;
            tbCost.Text = _item.Cost.ToString();
            tbStock.Text = _item.KolvoNaSklade.ToString();
            tbDiscount.Text = _item.Discount.ToString();

            cbCategory.SelectedValue = _item.IDTovarCategory;
            cbManufacturer.SelectedValue = _item.IDProizvoditelia;
            cbSupplier.SelectedValue = _item.IDPostavshik;
            cbUnit.SelectedValue = _item.IDEdIzm;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckForm())
                return;

            try
            {
                MapToEntity();
                _db.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.InnerException != null ? ex.InnerException.Message : ex.Message,
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool CheckForm()
        {
            if (!decimal.TryParse(tbCost.Text, out var cost) ||
                !int.TryParse(tbStock.Text, out var stock) ||
                !int.TryParse(tbDiscount.Text, out var discount))
            {
                MessageBox.Show("Ошибка: числа введены неверно");
                return false;
            }

            if (cost < 0 || stock < 0 || discount < 0)
            {
                MessageBox.Show("Ошибка: отрицательные значения недопустимы");
                return false;
            }

            if (cbCategory.SelectedValue == null ||
                cbManufacturer.SelectedValue == null ||
                cbSupplier.SelectedValue == null ||
                cbUnit.SelectedValue == null)
            {
                MessageBox.Show("Ошибка: заполните выпадающие списки");
                return false;
            }

            if (string.IsNullOrWhiteSpace(tbArticle.Text) || tbArticle.Text.Length != 6)
            {
                MessageBox.Show("Ошибка: артикул должен быть 6 символов");
                return false;
            }

            return true;
        }

        private void MapToEntity()
        {
            if (_item.IDTovar == 0)
            {
                _item.IDTovar = _db.Tovar.Any()
                    ? _db.Tovar.Max(x => x.IDTovar) + 1
                    : 1;

                _db.Tovar.Add(_item);
            }

            _item.TovarName = tbName.Text;
            _item.Description = tbDescription.Text;
            _item.Article = tbArticle.Text;

            _item.Cost = Convert.ToDecimal(tbCost.Text);
            _item.KolvoNaSklade = Convert.ToInt32(tbStock.Text);
            _item.Discount = Convert.ToInt32(tbDiscount.Text);

            _item.IDTovarCategory = (int)cbCategory.SelectedValue;
            _item.IDProizvoditelia = (int)cbManufacturer.SelectedValue;
            _item.IDPostavshik = (int)cbSupplier.SelectedValue;
            _item.IDEdIzm = (int)cbUnit.SelectedValue;
        }
    }
}