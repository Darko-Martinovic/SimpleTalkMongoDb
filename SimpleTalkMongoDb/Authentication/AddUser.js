// We will add the user and grant read and write permission.
//use simpleTalk

db.createUser(
    {
        user: "usrSimpleTalk",
        pwd: "pwdSimpleTalk",
        roles: [{ role: "readWrite", db: "simpleTalk" }]
    }
)