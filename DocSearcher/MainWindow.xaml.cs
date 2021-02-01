using Microsoft.Win32;
using System;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace DocSearcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string TERMS_COUNT_LABEL_FORMAT = "{0} terms";
        const string TERM_BREAKDOWN_LINE_FORMAT = "{0}:\t{1}\n";
        const string NOT_FOUND_TERM_BREAKDOWN_LINE_FORMAT = "<span Foreground=\"Maroon\">{0}:\t{1}</span>\n";
        string fileName = null;

        public MainWindow()
        {
            InitializeComponent();

            // Ensure that search term count is updated when the search terms box is loaded
            SearchTerms.Loaded += Search_Terms_Changed;

            SearchButton.IsEnabled = false;

        }

        private void Choose_File_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.DefaultExt = "*"; // Required file extension 

            bool? result = fileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                fileName = fileDialog.FileName;
            }
            else
            {
                fileName = null;
            }

            UpdateFileNameLabel();
        }

        private void UpdateFileNameLabel()
        {
            if (fileName != null)
            {
                FileNameLabel.Content = System.IO.Path.GetFileName(fileName);
                SearchButton.IsEnabled = true;
            }
            else
            {
                FileNameLabel.Content = "No file chosen.";
                SearchButton.IsEnabled = false;
            }
        }

        private void SearchTerms_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderLabel.Visibility = Visibility.Hidden;
        }

        private void SearchTerms_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTerms.Text == "")
            {
                PlaceholderLabel.Visibility = Visibility.Visible;
            }
        }

        private void Search_Terms_Changed(object sender, RoutedEventArgs e)
        {
            if (TermsCountLabel != null)
            {
                int count = ParseSearchTerms(SearchTerms.Text, false).Count;
                TermsCountLabel.Content = string.Format(TERMS_COUNT_LABEL_FORMAT, count);
            }
        }

        private List<string> ParseSearchTerms(string content, bool checkCaseSensitivity = true)
        {
            if (checkCaseSensitivity && CaseSensitiveCheckbox.IsChecked.HasValue && !CaseSensitiveCheckbox.IsChecked.Value)
            {
                content = content.ToLower();
            }

            
            
                // Convert to List to more easily remove potential empty strings
            List<string> searchTerms = new List<string>(content.Split(" \n\r\t,".ToCharArray()));
            searchTerms.RemoveAll(term => term == "");

            return searchTerms;
        }

        private void Search_Button_Click(object sender, RoutedEventArgs e)
        {
            List<string> searchTerms = ParseSearchTerms(SearchTerms.Text);
            int uniqueTermsCount = 0;

            // Create list of term counts
            List<int> searchTermCounts = new List<int>();

            if (fileName != null)
            {
                StreamReader sr = new StreamReader(fileName);
                string content = sr.ReadToEnd();
                if (CaseSensitiveCheckbox.IsChecked.HasValue && !CaseSensitiveCheckbox.IsChecked.Value)
                {
                    content = content.ToLower();
                }
                sr.Close();

                // Count occurences of terms
                for (int i = 0; i < searchTerms.Count; i++)
                {
                    int occurences = CountStringOccurrences(content, searchTerms[i]);
                    searchTermCounts.Add(occurences);
                    if (occurences != 0)
                    {
                        uniqueTermsCount++;
                    }
                } 
            }

            ResultsWindow resultsWindow = new ResultsWindow();

            resultsWindow.Owner = this;
            resultsWindow.TermsFoundLabel.Content = uniqueTermsCount;

            resultsWindow.TermsBreakdown.Text = "Count\tTerm\n";

            for (int i = 0; i < searchTerms.Count; i++)
            {
                if (searchTermCounts[i] != 0)
                {
                    resultsWindow.TermsBreakdown.Text += string.Format(TERM_BREAKDOWN_LINE_FORMAT, searchTermCounts[i], searchTerms[i]);
                }
            }

            if (resultsWindow.TermsBreakdown.Text == "Count\tTerm\n")
            {
                resultsWindow.TermsBreakdown.Text = "None of the terms were found.";
            }

            resultsWindow.ShowDialog();
        }

        public int CountStringOccurrences(string text, string pattern)
        {
            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }
    }
}
