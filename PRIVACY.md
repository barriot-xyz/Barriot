# 🔏 Privacy Policy

Please read this document carefully if there is any concern in how Barriot handles your information. 
This policy is automatically accepted as you make use of Barriot's services.

## 📤 Terminology

- _**Barriot**_
  - The Discord bot in question. This product is in care of Armano den Boef. The legal owner & only developer of Barriot.
- _**You, Member, User**_
  - You, a user of Barriot reading this privacy policy.
- _**Command, Commands**_
  - A command or multiple commands you can execute to which Barriot responds.
- _**Component, Components**_
  - A dropdown or button, or a collection of both, with which you can further interact with interactions.
- _**Entity, Entities**_
  - A data model that is used to store & retrieve specific information regarding a task executed by Barriot.
- _**Guild, Guilds**_
  - A Discord server, in Discord's API referred to as this definition.

## 📅 Data collection

### Reminders

Any created reminder will be stored in a collection, until it is used. 
Barriot does not keep unused data around. 
After a reminder is sent, it's data is pruned from the collection with no way of recovery.

### Polls

As with reminders, a collection is used to keep poll data around until it expires. 
After expiration, the data is pruned from the collection with no way of recovery.

### Interaction usage

Data is kept about your command & component usage. 
As a command is executed, a **command total** is incremented. 
As a button is pressed, a **component total** is incremented. 
After every command executed, it's name will be stored as your **latest command**. 
The latest command is replaced by its previous entry. Command details are not stored.

### Users

Each user has a unique ID provided by Discord. 
Barriot uses this ID to associate you with it's unique entities, such as polls & reminders.
Additionally, Barriot implements functions like translation which require a language to be specified prior to translation. 
These preferred options are defined along with others in Barriots user-entity collection

## 🗃️ Data storage

Data is stored in a MongoDB server, locally hosted on the same machine as Barriot itself. 
It is safely protected by a security manager & password system. 
The port MongoDB communicates over is locked externally, so no traffic from outside the machine can interfere with or keep tabs on your data. 
> If this security somehow fails, and your data is somehow grabbed or deleted, 
Barriot will notify you along other users of this event in its built-in notification system.

## ✅ Agreement

By adding Barriot to your guild or guilds, you agree to the above mentioned policy. 

## ❌ Disagreement

If you disagree with these terms as a guild owner or maintainer, you may remove Barriot from the guilds in question. 
If you disagree with these terms as a guild member, you may leave the guilds where Barriot resides.
