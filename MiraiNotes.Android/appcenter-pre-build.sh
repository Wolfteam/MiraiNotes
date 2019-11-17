#!/bin/bash
Client_ID='$Client_ID'
Client_Secret='$Client_Secret'
Client_RedirectUrl='$Client_RedirectUrl'
Client_AppCenterSecret='$Client_AppCenterSecret'
Client_DB_IV='$Client_DB_IV'
Client_DB_PASSWORD='$Client_DB_PASSWORD'

CLASS="namespace MiraiNotes.Shared
{
    public class Secrets
    {
        /*Google secrets*/
#if Android
        public const string ClientId = $Client_ID;
        public const string ClientSecret = $Client_Secret;
        public const string RedirectUrl = $Client_RedirectUrl;
        public const string AppCenterSecret = $Client_AppCenterSecret;
#else
        public const string ClientId = $Client_ID;
        public const string ClientSecret = $Client_Secret;
        public const string RedirectUrl = $Client_RedirectUrl;
        public const string AppCenterSecret = $Client_AppCenterSecret;
#endif

        /*DbSecrets
         This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
         32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.*/
        public const string InitVector = $Client_DB_IV;

        /*This constant is used to determine the keysize of the encryption algorithm*/
        public const int KeySize = 256;

        public const string Password = $Client_DB_PASSWORD;
    }
}"
# print the contents of the variable on screen
echo $CLASS > ../MiraiNotes.Shared/Secrets.cs