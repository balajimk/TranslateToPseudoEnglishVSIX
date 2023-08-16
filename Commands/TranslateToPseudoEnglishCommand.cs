using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TranslateToPseudoEnglish
{
    [Command(PackageIds.TranslateToPseudoEnglishCommand)]
    internal sealed class TranslateToPseudoEnglishCommand : BaseCommand<TranslateToPseudoEnglishCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            try
            {
                var docview = await VS.Documents.GetActiveDocumentViewAsync();
                foreach (var virtualSnapshotSpan in docview?.TextView.Selection.VirtualSelectedSpans)
                {
                    string textToTranslate = virtualSnapshotSpan.GetText();

                    string translatedText = Translate(textToTranslate);
                    translatedText = RevertItems(translatedText, GetGUIDs(textToTranslate));
                    translatedText = RevertItems(translatedText, GetHTMLTags(textToTranslate));

                    docview.TextBuffer.Replace(virtualSnapshotSpan.SnapshotSpan.Span, translatedText);
                }
                await VS.StatusBar.ShowMessageAsync("Pseudo-English translation completed successfully.");
            }
            catch (Exception ex)
            {
                await VS.StatusBar.ShowMessageAsync($"Pseudo-English translation failed with exception: {ex.Message}");
                await ex.LogAsync();
            }
        }

        private static string Translate(string text)
        {
            const string pseudoLower = @"áط¢đεƒgիίذкլ๓ดơρཤяštůvωχЎž";
            const string pseudoUpper = @"Á฿ĆĐЁϜGнΙلΚԼ៣ЙФP이ЯនTŮ٧ฟჯΫŽ";
            string result = text;

            for (int i = 0; i <= 25; i++)
            {
                result = result.Replace((Char)(i + 65), pseudoUpper[i]).Replace((Char)(i + 97), pseudoLower[i]);
            }

            return result;
        }

        private static string RevertItems(string translated, List<string> items)
        {
            string result = translated;

            foreach (string item in items)
            {
                result = RevertTranslation(result, item);
            }

            return result;
        }

        private static string RevertTranslation(string translated, string enTextToRevert)
        {
            string tagTranslation = Translate(enTextToRevert);
            string result = translated.Replace(tagTranslation, enTextToRevert);

            return result;
        }

        private static List<string> GetGUIDs(string text)
        {
            List<string> result = new();
            MatchCollection matchCollection;
            string pattern = "([a-z0-9]{8}[-][a-z0-9]{4}[-][a-z0-9]{4}[-][a-z0-9]{4}[-][a-z0-9]{12})";

            matchCollection = Regex.Matches(text, pattern);
            foreach (var mci in matchCollection)
            {
                result.Add(mci.ToString());
            }

            return result;
        }

        /// <summary>
        /// This gives a list of HTML tags. 
        /// This is still not the best since it will ignore the attribiute values.
        /// We need to use a HTML parser instead.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static List<string> GetHTMLTags(string text)
        {
            List<string> result = new();
            MatchCollection matchCollection;
            string pattern = "<(?:\"[^\"]*\"['\"]*|'[^']*'['\"]*|[^'\">])+>";

            matchCollection = Regex.Matches(text, pattern);
            foreach (var mci in matchCollection)
            {
                string? value = mci?.ToString();
                if (!string.IsNullOrEmpty(value)) result.Add(value);
            }

            return result;
        }
    }
}
