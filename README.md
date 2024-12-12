# LCB Remote

[![🧪 Tested with 7DTD 1.2 (b27)](https://img.shields.io/badge/🧪%20Tested%20with-7DTD%201.2%20(b27)-blue.svg)](https://7daystodie.com/)
[![✅ Dedicated Servers Supported ServerSide](https://img.shields.io/badge/✅%20Dedicated%20Servers-Supported%20Serverside-blue.svg)](https://7daystodie.com/)
[![❌ Single Player and P2P Unupported](https://img.shields.io/badge/❌%20Single%20Player%20and%20P2P-Unsupported-red.svg)](https://7daystodie.com/)
[![📦 Automated Release](https://github.com/jonathan-robertson/lcb-remote/actions/workflows/release.yml/badge.svg)](https://github.com/jonathan-robertson/lcb-remote/actions/workflows/release.yml)

## Summary

7 Days to Die mod: Add admin tools and a craftable remote control to turn on/off land claim block boundaries.

## PLANNED Player Features

Adds a craftable remote control that can turn on/off the closest owned land claim block boundaries the player is within range of.

## Admin Commands

This mod adds commands for checking and adjusting any land claim block's activation state.

> ℹ️ When adjusted, the mod will send packages that trick the client into reloading that land claim block's state for the owner (if the owner is online and within range). This means that the land claim owner will not need to log in/out to see the boundaries for this claim appear or disappear when the block's state is modified by the admin.

Command | Description
--- | ---
`lcbr check` | Check the current activation state of the lcb you are within range of.
`lcbr activate` | Activate lcb area frame for the lcb you are within range of (only the lcb owner will see it).
`lcbr deactivate` | Deactivate lcb area frame for the lcb you are within range of.

> ℹ️ When the `activate` or `deactivate` command is run, the land claim block's owner will see an immediate change to the boundaries without having to leave/return or logout/login.
