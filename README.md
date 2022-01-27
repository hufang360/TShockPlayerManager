# TShockPlayerManager

导出玩家存档，查看玩家背包。


# 部分指令
```
/ppm help, 帮助

/ppm look <玩家名>, 查看玩家
/lookbag <玩家名>, 查看玩家（普通用户）

/ppm export <玩家名>, 导出单个玩家存档
/ppm exportall, 导出全部玩家存档

/ppm maxhp <玩家名> <生命上限>, 修改生命上限
/ppm maxmana <玩家名> <魔力上限>, 修改魔力上限
/ppm hp <玩家名>, 修改生命值
/ppm mana <玩家名>, 修改魔力值


// 主指令的其它写法
/ppm = /playermanager
.ppm = /ppm

// 简写
/ppm look = /ppm l
/ppm export = /ppm e
/ppm exportall = /ppm ea
/ppm maxhp = /ppm mh
/ppm maxmana = /ppm mm
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