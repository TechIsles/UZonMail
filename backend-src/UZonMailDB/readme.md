# ��Ŀ˵��

��Ŀ¼�������ݿ����صĶ��� model,Ŀǰ���� sqlLite,�����������л��� mysql,���������չ

## ����Ǩ��˵��

z uzonmaildb

1. Mysql

dotnet ef migrations add addSmtpInfo --context MysqlContext --output-dir Migrations/Mysql -v

2. SqLite

dotnet ef migrations add addSmtpInfo --context SqLiteContext --output-dir Migrations/SqLite -v