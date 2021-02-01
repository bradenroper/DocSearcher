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
        /// <summary>
        /// Format used when counting the number of terms being searched.
        /// </summary>
        const string TERMS_COUNT_LABEL_FORMAT = "{0} terms";

        /// <summary>
        /// Format used to show the terms and their counts in the results view.
        /// </summary>
        const string TERM_BREAKDOWN_LINE_FORMAT = "{0}\t{1}\n";

        /// <summary>
        /// Filename of the document to be searched. This is set by the file 
        /// chooser and kept until the search is triggered.
        /// </summary>
        string fileName = null;


        /// <summary>
        /// Initialize the main window.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Ensure that search term count is updated when the search terms box is loaded
            SearchTerms.Loaded += Search_Terms_Changed;

            // Disable search button initially, choosing a file will enable this
            SearchButton.IsEnabled = false;
        }

        /// <summary>
        /// On click of the choose file button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Updates the chosen file label.
        /// </summary>
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

        /// <summary>
        /// On focus of the search terms box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTerms_GotFocus(object sender, RoutedEventArgs e)
        {
            // Hide the gray, italicized placeholder text
            PlaceholderLabel.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// On unfocus of the search terms box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTerms_LostFocus(object sender, RoutedEventArgs e)
        {
            // Reshow the gray, italicized placeholder text if the box is empty
            if (SearchTerms.Text == "")
            {
                PlaceholderLabel.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// On change of the search terms box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Terms_Changed(object sender, RoutedEventArgs e)
        {
            // Recalculate the number of search terms
            if (TermsCountLabel != null)
            {
                int count = ParseSearchTerms(SearchTerms.Text, false).Count;
                TermsCountLabel.Content = string.Format(TERMS_COUNT_LABEL_FORMAT, count);
            }
        }

        /// <summary>
        /// Parse the search terms and return them as a list of strings.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="checkCaseSensitivity"></param>
        /// <returns></returns>
        private List<string> ParseSearchTerms(string content, bool checkCaseSensitivity = true)
        {
            // If case sensitivity is not set, set content all to lowercase
            if (checkCaseSensitivity && CaseSensitiveCheckbox.IsChecked.HasValue && !CaseSensitiveCheckbox.IsChecked.Value)
            {
                content = content.ToLower();
            }

            // Split terms using a few delimiters
            List<string> searchTerms = new List<string>(content.Split("\n\r\t".ToCharArray()));
            searchTerms.RemoveAll(term => term == "");

            return searchTerms;
        }

        /// <summary>
        /// On click of the search button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Button_Click(object sender, RoutedEventArgs e)
        {
            // Get list of terms to search for
            List<string> searchTerms = ParseSearchTerms(SearchTerms.Text);
            
            // Create list of term counts
            List<int> searchTermCounts = new List<int>();
            
            // Keep track of unique terms found
            int uniqueTermsCount = 0;

            if (fileName != null)
            {
                // Read file
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

            // Initialize and configure results window
            ResultsWindow resultsWindow = new ResultsWindow();
            resultsWindow.Owner = this;
            
            // Set unique terms label
            resultsWindow.TermsFoundLabel.Content = uniqueTermsCount;

            // Set results breakdown text box
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

            // Spawn the window
            resultsWindow.ShowDialog();
        }

        /// <summary>
        /// Helper function to count string occurences.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public int CountStringOccurrences(string text, string pattern)
        {
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
