# ��Ŀ˵��

��Ŀ¼�������ݿ����صĶ��� model,Ŀǰ���� sqlLite,�����������л��� mysql,���������չ

## ����Ǩ��˵��

z uzonmaildb

1. Mysql

dotnet ef migrations add perfSendingItemInbox --context MysqlContext --output-dir Migrations/Mysql -v

2. SqLite

dotnet ef migrations add perfSendingItemInbox --context SqLiteContext --output-dir Migrations/SqLite -v

## ȡ������Ǩ��

1. Mysql

dotnet ef migrations remove --context MysqlContext -v

2. SqLite

dotnet ef migrations remove --context SqLiteContext -v