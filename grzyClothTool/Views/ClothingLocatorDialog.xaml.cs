using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace grzyClothTool.Views
{
    public partial class ClothingLocatorDialog : Window
    {
        // GTA V Base clothing counts as provided by the user
        private readonly Dictionary<string, int> _baseCounts = new()
        {
            { "berd", 238 }, // 0 to 237
            { "teef", 148 }, // 0 to 147
            { "jbib", 565 }, // 0 to 564
            { "accs", 253 }, // 0 to 252
            { "task", 62 },  // 0 to 61
            { "hand", 111 }, // 0 to 110
            { "lowr", 207 }, // 0 to 206
            { "feet", 154 }, // 0 to 153
            { "decl", 209 }  // 0 to 208
        };

        private readonly Dictionary<string, string> _categoryNames = new()
        {
            { "berd", "berd (Decals/Badges)" },
            { "teef", "teef (Accessories/Teeth)" },
            { "jbib", "jbib (Tops/Shirts)" },
            { "accs", "accs (Accessories)" },
            { "task", "task (Tasks/Armor)" },
            { "hand", "hand (Hands/Bags)" },
            { "lowr", "lowr (Pants)" },
            { "feet", "feet (Shoes)" },
            { "decl", "decl (Decals)" }
        };

        private const int ITEMS_PER_ADDON = 128;

        public string ResultModdedID { get; private set; } = string.Empty;
        public int ResultAddonNumber { get; private set; } = 0;
        public bool IsSearchRequested { get; private set; } = false;

        public ClothingLocatorDialog()
        {
            InitializeComponent();
            PopulateCategories();
        }

        private void PopulateCategories()
        {
            foreach (var kvp in _categoryNames)
            {
                CategoryComboBox.Items.Add(new ComboBoxItem
                {
                    Content = kvp.Value,
                    Tag = kvp.Key
                });
            }

            if (CategoryComboBox.Items.Count > 0)
            {
                CategoryComboBox.SelectedIndex = 0;
            }
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string categoryKey)
            {
                if (_baseCounts.TryGetValue(categoryKey, out int count))
                {
                    BaseCountTextBox.Text = count.ToString();
                    CalculateResult();
                }
            }
        }

        private void TargetIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Only allow numbers
            TargetIdTextBox.Text = Regex.Replace(TargetIdTextBox.Text, "[^0-9]", "");
            TargetIdTextBox.CaretIndex = TargetIdTextBox.Text.Length;
            
            CalculateResult();
        }

        private void CalculateResult()
        {
            ResultBorder.Visibility = Visibility.Collapsed;
            ErrorBorder.Visibility = Visibility.Collapsed;
            SearchButton.IsEnabled = false;
            ResultModdedID = string.Empty;
            ResultAddonNumber = 0;

            if (string.IsNullOrWhiteSpace(TargetIdTextBox.Text) || string.IsNullOrWhiteSpace(BaseCountTextBox.Text))
            {
                return;
            }

            if (int.TryParse(TargetIdTextBox.Text, out int targetId) && int.TryParse(BaseCountTextBox.Text, out int baseCount))
            {
                if (targetId < baseCount)
                {
                    ErrorText.Text = $"Target ID ({targetId}) is less than the base count ({baseCount}). This is a base GTA clothing item, not a modded one.";
                    ErrorBorder.Visibility = Visibility.Visible;
                }
                else
                {
                    int rawModdedId = targetId - baseCount;
                    int addonNumber = (rawModdedId / ITEMS_PER_ADDON) + 1; // 1-based addon number
                    int itemInAddon = rawModdedId % ITEMS_PER_ADDON;
                    string formattedId = itemInAddon.ToString("D3");

                    // Get category key for display
                    string categoryKey = "";
                    if (CategoryComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string key)
                    {
                        categoryKey = key;
                    }

                    ResultText.Text = $"Addon {addonNumber} → Item: {formattedId}";
                    ResultDetailText.Text = $"Search for: {categoryKey}_{formattedId}_u  (Addon #{addonNumber} of loaded addons)";
                    ResultBorder.Visibility = Visibility.Visible;

                    ResultModdedID = formattedId;
                    ResultAddonNumber = addonNumber;
                    SearchButton.IsEnabled = true;
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ResultModdedID))
            {
                IsSearchRequested = true;
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
