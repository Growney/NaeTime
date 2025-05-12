using ELRS.Backpack;
using System.Security.Cryptography;
using System.Text;

 byte[] HashPhrase(string bindPhrase)
 {
     // Create the input string
     string input = $"-DMY_BINDING_PHRASE=\"{bindPhrase}\"";

     // Compute the MD5 hash
     using MD5 md5 = MD5.Create();
     byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

     // Take the first 6 bytes of the hash
     byte[] bindingPhraseHash = hash.Take(6).ToArray();

     // Adjust the first byte if it is odd
     if ((bindingPhraseHash[0] % 2) == 1)
     {
         bindingPhraseHash[0] -= 0x01;
     }

     return bindingPhraseHash;
 }

var serialiser = new BackpackCommandSerializer();
var backpackConnectionFactory = new BackpackConnectionFactory(serialiser);
var connection = backpackConnectionFactory.Create("COM7");

byte[] uId = HashPhrase("bindingphrasehere");

await connection.SetOSDElement(uId,"HELLO", OSDPresentation.Info, 3,0,TimeSpan.FromSeconds(5));