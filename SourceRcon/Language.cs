using System;
using System.Collections.Generic;
using System.Text;

namespace SourceRcon
{
    abstract internal class ILanguage
    {
        ILanguage defaultLang;
        public ILanguage DefaultLanguage
        {
            get
            {
                return defaultLang;
            }
            set
            {
                defaultLang = value;
            }
        }

        public string this[string s]
        {
            get
            {
                string t = null;

                try
                {
                    t = GetTranslation(s);
                }
                catch
                {
                }

                if (t == null || t == String.Empty)
                {
                    // Get the default tranlation instead.
                    try
                    {
                        t = defaultLang.GetTranslation(s);
                    }
                    catch
                    {
                    }

                    if (t == null || t == String.Empty)
                    {
                        return s;
                    }
                }

                return t;
            }
        }

        abstract protected string GetTranslation(string s);
    }

    internal class English : ILanguage
    {
        public English()
        {
            dict = new Dictionary<string, string>();

            dwrite("usage_instructions", "\nTo use SourceRcon in interactive mode, use no parameters.\n" +
                    "Otherwise, use parameters in the form: ip port password command\n" +
                    "Enclose the command in \" marks if it is more than one word.\n" +
                    "If you need to use \" marks in the command itself, escape them by writing \n" +
                    "\\\" instead. \n\n" +
                    @"E.g. sourcercon 192.168.0.5 27015 testpass ""say \""Hello World!\"""" ");

            dwrite("enterip", "Enter IP Address:");
            dwrite("enterport", "Enter port:");
            dwrite("enterpassword", "Enter password:");
            dwrite("commandready", "Ready for commands:");
            dwrite("noconn", "No connection!");
            dwrite("error", "Error: {0}");
            dwrite("console", "Console: {0}");
            dwrite("invalidparams", "Invalid parameters: {0}");

            /*
             * Old strings, will maybe reincorperate them in future.
             * 
public static string ConnectionClosed = "Connection closed by remote host";
public static string ConnectionSuccessString = "Connection Succeeded!";
public static string ConnectionFailedString = "Connection Failed!";
public static string UnknownResponseType = "Unknown response";
public static string GotJunkPacket = "Had junk packet. This is normal.";
*/

        }

        Dictionary<string,string> dict;

        protected override string GetTranslation(string s)
        {
            return dict[s];
        }

        public void dwrite(string key, string value)
        {
            dict.Add(key,value);
        }
    }

    // Localisation strings translated by Google translate. So probably not very good.
    internal class GoogleFrench : ILanguage
    {
        protected override string GetTranslation(string s)
        {
            return dict[s];
        }

        public GoogleFrench()
        {
            dict = new Dictionary<string, string>();

            dwrite("usage_instructions", "Pour l'utiliser en mode interactif, ne pas utiliser de paramètres. \n" + 
                     "Else utiliser les paramètres de la forme: ip port de passe commande \n "+ 
                     "Joignez la commande \" marques si elle est plus d'un mot \n "+ 
                     "Par exemple, sourcercon 192.168.0.5 27015 testpass \"say Testing!\" ");

            dwrite("enterip", "Entrez l'adresse IP:");
            dwrite("enterport", "Entrez le port:");
            dwrite("enterpassword", "Entrez le mot de passe:");
            dwrite("commandready", "Prêt pour les commandes:");
            dwrite("noconn", "Pas de connexion!");
            dwrite("error", "Error: {0}");
            dwrite("console", "Console: {0}");

        }

        Dictionary<string,string> dict;

        public void dwrite(string key, string value)
        {
            dict.Add(key,value);
        }

    }
}
