//use simpleTalk

db.createUser(
    {
        user: "usrSimpleTalk",
        pwd: "pwdSimpleTalk",
        roles: [{ role: "readWrite", db: "simpleTalk" }]
    }
)