using System.Diagnostics;

namespace Ona_Pix
{
    internal static class Define
    {
        #region URL的正则表达式 (URL_REGEX)
        internal const string URI_REGEX = @"^(((ht|f)tp(s?))\://)?" +
                                        @"([a-zA-Z].)[a-zA-Z0-9\-\.]+\.(" +
                                        @"com|edu|gov|mil|net|org|biz|info|name|museum|us|ca|uk|cc|int|arpa|asia|pro|coop|aero|tv|top|xin|xyz|vip|cn|mobi|ru|de|pl|eu|io|jp|club|au|post|me|guru|expert|tw|mo|hk|fr|ar|pk|mv|in|it|ws|sh|my|cd|ac|li|co|cm|win|red|rec|travel|wang|ch|dj|er|ee|es|is|kr|mm|mn|no|ne|to|tr|za|ml|ga|xxx|porn|adult|cyou|buzz|monster|icu|shop|best|site|live|online|pw|cloud|website|life|store|fun|app|today|uno|space|world|one|link|work|email|nl|trade|bid|stream|men|art|party|date|dev|tech|church|rocks|digital|download|moe|agency|network|click|fail|news|cool|vegas|blog|review|company|la|design|services|golf|exposed|team|ltd|help|zone|loan|wtf|media|solutions|games|directory|center|care|fyi|group|ooo|science|systems|works|city|pet|run|tips|studio|guide|cash|at|support|ninja|plus|rip|marketing|vin|reisen|tools|finance|immo|wiki|viajes|global|mx|promo|recipes|photos|academy|dog|pink|money|chat|casa|cafe|ink|foundation|faith|webcam|house|technology|photography|video|blue|farm|codes|realty|tel|land|show|business|international|social|sbs|skin|page|london|health|hiv|bond|vote|autos|rest|limo|hospital|gay|bar|game|fans|ph|study|cooking|glass|place|rent|shoes|tires|wedding|cab|camp|beer|menu|movie|fish|sexy|gifts|part|mom|green|moda|baby|navy|loans|engineering|computer|camera|barga|film|kitchen|supply|pics|haus|university|fit|cheap|vet|black|law|lol|tax|bio|sale|earth|kim|love|software|fitness|llc|school|pub|deals|style|domains|band|host|direct|shopping|tube|ist|mba|photo|energy|training|taxi|gift|wine|town|bike|toys|ski|poker|yoga|parts|solar|engineer|garden|observer|sucks|hosting|bingo|christmas|gives|horse|insure|diet|fishing|whoswho|tickets|boats|flowers|storage|cfd|inc|quest|luxe|security" +
                                        @")(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\;\?\'\\\+&amp;%\$#\=~_\-]+))*$";
        #endregion URL的正则表达式 (URL_REGEX)
        internal static readonly string[] fileSuffixes = { ".png", ".jpg", ".gif" };

        internal static void StartProcess(string fileName)
        {
            ProcessStartInfo processStartInfo = new(fileName) { UseShellExecute = true };
            Process.Start(processStartInfo);
        }
    }
}