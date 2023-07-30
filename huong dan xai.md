# chay file .bat hoac copy code ben duoi paste vao terminal 
## Lay 1 store ko co tham so
GenSP.exe scan -cnn "Server=TUNGMTP\SQLEXPRESS;Database=MES_DEV;MultipleActiveResultSets=true;User Id=sa;Password=123;TrustServerCertificate=True" -nsp API -sch PRD_GET_MPS_WORKCENTER_UA -ctx "DBcontext" -sf "Model" -pf D:\Project\GenSPTool\ -f "ResultSingle.cs"

## 1 store co tham so
GenSP.exe scan -cnn "Server=TUNGMTP\SQLEXPRESS;Database=MES_DEV;MultipleActiveResultSets=true;User Id=sa;Password=123;TrustServerCertificate=True" -nsp API -sch P_Get_Each_PS_And_WorkCenter_Calling -ctx "DBcontext" -sf "Model" -pf D:\Project\GenSPTool\ -f "ResultSingle.cs"

## Lay all store
GenSP.exe scan -cnn "Server=TUNGMTP\SQLEXPRESS;Database=MES_DEV;MultipleActiveResultSets=true;User Id=sa;Password=123;TrustServerCertificate=True" -nsp API -sch * -ctx "DBcontext" -sf "Model" -pf D:\Project\GenSPTool\ -f "ResultAll.cs"

## cu phap: 
GenSP.exe scan
**-cnn** connectionString 
**-nsp** Namespace (IsRequired = false, default = API)
**-sch** * (sao la lay het, ko muon lay het thi ghi ten store vao) 
**-ctx** "DBcontext" (IsRequired = false, default ="DBContext")
**-sf** "thu muc Model" (IsRequired = false, default ="Models")
**-pf** "duong dan xuat file" (IsRequired = false, default ="current folder")
**-f** "fileResult.cs" (name bat ky) (IsRequired = false, default ="Result")

# option
GenSP.exe --connection "your_connection_string" --schema "YourSchema"

# full: 
GenSP.exe --connection "your_connection_string" --namespace "YourNamespace" --schema "YourSchema" --context "YourContext" --sfolder "Model" --pfolder "C:\YourSolution\Models" --filename "GenSPContext.cs"

