#!/bin/bash
ID="$Client_ID"
Secret="$Client_Secret"
RedirectUrl="$Client_RedirectUrl"
AppCenterSecret="$Client_AppCenterSecret"
DB_IV="$Client_DB_IV"
DB_PASSWORD="$Client_DB_PASSWORD"

if [ -z "$ID" ]
then
    echo "You need define the Client_ID variable in App Center"
    exit
fi

if [ -z "$Secret" ]
then
    echo "You need define the Client_Secret variable in App Center"
    exit
fi

if [ -z "$RedirectUrl" ]
then
    echo "You need define the Client_RedirectUrl variable in App Center"
    exit
fi

if [ -z "$AppCenterSecret" ]
then
    echo "You need define the Client_AppCenterSecret variable in App Center"
    exit
fi

if [ -z "$DB_IV" ]
then
    echo "You need define the Client_DB_IV variable in App Center"
    exit
fi

if [ -z "$DB_PASSWORD" ]
then
    echo "You need define the Client_DB_PASSWORD variable in App Center"
    exit
fi

CLASS="namespace MiraiNotes.Shared
{
    public class Secrets
    {
        /*Google secrets*/
#if Android
        public const string ClientId = "\"$ID\"";
        public const string ClientSecret = "\"$Secret\"";
        public const string RedirectUrl = "\"$RedirectUrl\"";
        public const string AppCenterSecret = "\"$AppCenterSecret\"";
#else
        public const string ClientId = "\"$ID\"";
        public const string ClientSecret = "\"$Secret\"";
        public const string RedirectUrl = "\"$RedirectUrl\"";
        public const string AppCenterSecret = "\"$AppCenterSecret\"";
#endif

        /*DbSecrets
         This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
         32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.*/
        public const string InitVector = "\"$DB_IV\"";

        /*This constant is used to determine the keysize of the encryption algorithm*/
        public const int KeySize = 256;

        public const string Password = "\"$DB_PASSWORD\"";
    }
}"

echo "Saving Secrets.cs file into MiraiNotes.Shared"
echo "$CLASS"

# print the contents of the variable on screen,
# we use "" to keep the line breaks
echo "$CLASS" > ../MiraiNotes.Shared/Secrets.cs