# balance-tracker-backend

Generating pem keys for encryption:
1. [Optional] Create file with private key passphrase
2. openssl genrsa [-aes128 -passout file:passphrase.txt] -out PrivateKey.pem 1024
3. openssl rsa -in PrivateKey.pem [-passin file:passphrase.txt] -pubout -out PublicKey.pem
4. Fill in Encryption section of appsettings.json