## Asp.net 修改`SessionId`的方法
- 方法1. 当Session没有保存信息, 比如没有使用`Session['name']`之类的, 则 `SessionId`在页面刷新和重新打开都会产生新的`SessionId`, 但是一旦使用了`Session['name']`, 则产生的`SessionId`就不改变

- 方法2. 动态修改`SessionId`, 使用到重新获取和去除`sessionId`的方法, 参考:https://stackoverflow.com/questions/15241464/after-change-sessionid-data-in-session-variables-is-lost

动态改变sessionId方法如下:
```cs
private string ReGenerateSessionId() {
    var Context = ((HttpApplication)HttpContext.GetService(typeof(HttpApplication))).Context;
    SessionIDManager manager = new SessionIDManager();
    string oldId = manager.GetSessionID(Context);
    string newId = manager.CreateSessionID(Context);
    bool isAdd = false, isRedir = false;
    manager.RemoveSessionID(Context);
    manager.SaveSessionID(Context, newId, out isRedir, out isAdd);
    
    HttpApplication ctx = (HttpApplication)HttpContext.ApplicationInstance;
    HttpModuleCollection mods = ctx.Modules;
    System.Web.SessionState.SessionStateModule ssm = (SessionStateModule)mods.Get("Session");
    System.Reflection.FieldInfo[] fields = ssm.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
    SessionStateStoreProviderBase store = null;
    System.Reflection.FieldInfo rqIdField = null, rqLockIdField = null, rqStateNotFoundField = null;
    
    SessionStateStoreData rqItem = null;
    foreach (System.Reflection.FieldInfo field in fields) {
        if (field.Name.Equals("_store")) store = (SessionStateStoreProviderBase)field.GetValue(ssm);
        if (field.Name.Equals("_rqId")) rqIdField = field;
        if (field.Name.Equals("_rqLockId")) rqLockIdField = field;
        if (field.Name.Equals("_rqSessionStateNotFound")) rqStateNotFoundField = field;
    
        if ((field.Name.Equals("_rqItem"))) {
            rqItem = (SessionStateStoreData)field.GetValue(ssm);
        }
    }
    object lockId = rqLockIdField.GetValue(ssm);
    
    if ((lockId != null) && (oldId != null)) {
        store.RemoveItem(Context, oldId, lockId, rqItem);
    }
    
    rqStateNotFoundField.SetValue(ssm, true);
    rqIdField.SetValue(ssm, newId);
    
    return newId;
}
```

> 感谢[@wennercn](https://github.com/wennercn) 的帮助