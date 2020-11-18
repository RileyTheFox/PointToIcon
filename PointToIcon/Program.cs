using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Resources;
using Newtonsoft.Json;
using System.Net;
using System.Drawing.Imaging;
using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace PointToIcon
{

    /*
     * There's tons of stuff in here that needs proper error handling in case of
     * unexpected situations but I haven't encountered any problems and it was just
     * a small joke program so I'm not really that bothered.
     */
    public class Program
    {
        // Some constants for using the Win32 API.
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        private static List<Position> Positions;
        private static NamedDesktopPoint[] DesktopIcons;

        private static int ScreenWidth;
        private static int ScreenHeight;

        private static Position ScreenSize;

        static void Main()
        {
            // Some global setup stuff
            ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
            ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
            ScreenSize = new Position(ScreenWidth, ScreenHeight);

            // Fetch the embedded positions resource
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "PointToIcon.new-positions.json";

            string result = string.Empty;

            // Read it from a stream
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            // Create some position objects from the json array.
            List<float[]> jsonObject = JsonConvert.DeserializeObject<List<float[]>>(result);
            Positions = new List<Position>();
            jsonObject.ForEach(i =>
            {
                Positions.Add(new Position(i[0], i[1]));
            });

            Desktop desktop = new Desktop();

            DesktopIcons = desktop.GetIconsPositions();

            // Setup speech recognition
            using (SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-GB"))) // bri un
            {
                // Not sure what a lot of this does, it's just the example in the Microsoft Docs
                recognizer.LoadGrammar(new DictationGrammar());

                recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Recognizer_SpeechRecognized);

                recognizer.SetInputToDefaultAudioDevice();
                recognizer.RecognizeAsync(RecognizeMode.Multiple);

                // Will close the program on the next key press in the console
                Console.ReadKey();
            }
        }

        // Tries to find the an icon that best suits what the user spoke.
        private static int GetSuitableIconIndex(RecognitionResult result)
        {
            string text = result.Text;
            text = RemoveSpecialChars(text).ToLower();

            // Loop through all the icons
            for (int i = 0; i < DesktopIcons.Length; i++)
            {
                NamedDesktopPoint icon = DesktopIcons[i];

                string iconNameLower = icon.Name.ToLower();

                // This removes the file extension because saying the file extension would be very hard to recognise
                if(iconNameLower.LastIndexOf('.') > 0)
                    iconNameLower = iconNameLower.Substring(0, iconNameLower.LastIndexOf('.'));

                iconNameLower = RemoveSpecialChars(iconNameLower);

                // If the icon name perfectly matches just return the icon but this probably won't happen so we need to use homophones
                if(iconNameLower.Contains(text)) 
                {
                    return i;
                }

                // Loop through all the homophones in the speech result
                foreach(RecognizedPhrase phrase in result.Homophones)
                {
                    // This will most likely return the icon at some point because the homophones are just
                    // words/sentences the speech recognition thinks you might have said, but wasn't 100% sure.
                    if(iconNameLower.Contains(RemoveSpecialChars(phrase.Text.ToLower()))) 
                    {
                        return i;
                    }
                }
            }

            // -1 means nothing was found
            return -1;
        }

        // Called when speech is recognised
        private static void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            int iconIndex = GetSuitableIconIndex(e.Result);

            if(iconIndex != -1)
            {
                NamedDesktopPoint icon = DesktopIcons[iconIndex];
                Console.WriteLine(icon.Name);

                Position iconPosition = new Position(icon.X + 20, icon.Y + 50);

                // The image numbers correspond exactly to the order of positions in the positions array so it can be used to get the correct image.
                int imageIndex = ClosestImage.UseClosest(ScreenSize, iconPosition, Positions);

                // Idea for this came from this https://pointerpointer.com where it points to your mouse.
                // I'm just using all the images stored there and the positions (it's much easier)
                Image image = GetImageFromURL($"https://pointerpointer.com/images/{imageIndex}.jpg");
                string tempPath = Path.Combine(Path.GetTempPath(), "pointerwallpaper.bmp");
                // Save the file as a bmp so it can be set as a background
                image.Save(tempPath, ImageFormat.Bmp);

                // Calls the Win32 API to change the desktop background
                Win32.SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

            }
        }

        // Opens a stream from the URL and gets an image from it.
        public static Image GetImageFromURL(string url)
        {
            WebClient client = new WebClient();
            Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var imageUri);
            return Image.FromStream(client.OpenRead(imageUri));
        }

        // Remove anything that is not a letter or number from a string
        private static string RemoveSpecialChars(string @string)
        {
            return @string.Where(char.IsLetterOrDigit).Aggregate(new StringBuilder(), (s, c) => s.Append(c)).ToString();
        }
    }
}
