# TShockPlayerManager

导出玩家存档，查看玩家背包。


# 部分指令
```
/pm help, 帮助

/pm look <玩家名>, 查看玩家
/lookbag <玩家名>, 查看玩家（普通用户）

/pm export <玩家名>, 导出单个玩家存档
/pm exportall, 导出全部玩家存档

/pm maxhp <玩家名> <生命上限>, 修改生命上限
/pm maxmana <玩家名> <魔力上限>, 修改魔力上限
/pm hp <玩家名>, 修改生命值
/pm mana <玩家名>, 修改魔力值


// 主指令的其它写法
/pm = /playermanager
.pm = /pm

// 简写
/pm look = /pm l
/pm export = /pm e
/pm exportall = /pm ea
/pm maxhp = /pm mh
/pm maxmana = /pm mm
```

# 权限
```
playermanager
lookbag
```

开放查背包功能
```
/group addperm default lookbag
```


# 说明
导出玩家数据部分，使用了此项目的部分代码： 
https://github.com/Megghy/PlayerExport