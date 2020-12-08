# SATools <a href="https://github.com/BlackMesaDude/SATools/releases"><img src="https://img.shields.io/badge/Version-1.0-informational" /></a> <a href="https://github.com/BlackMesaDude/SATools/blob/main/LICENSE"><img src="https://img.shields.io/badge/License-GPL%20v2.0-informational" /></a> ![.NET Core](https://github.com/BlackMesaDude/SATools/workflows/.NET%20Core/badge.svg)

## Index
<!--ts-->
   * [Introduction](#SATools)
      * [Why this library](#why-this-library)
      * [What's out of the box?](#whats-out-of-the-box)
      * [Notes on the support](#notes-on-the-support)
   * [Offerings](#offerings)
   * [How it works](#how-it-works)
   * [The IMG Archive](#the-img-archive)
       * [Header](#header)
       * [Directory Entry (Block)](#directory-entry-block)
<!--te-->

**SATools** (or *San Andreas Tools*) is a ```C#.Net``` based library made for the next mod developer that wants an easy way to access **GTA:SA resources**, such as archives or backend scipting files.

## Why this library? 

This library, as said before, is meant to help the mod. developer with his work by allowing him the use of a **CLR language as an intermediary** (in this case **C#**). This allows the creator to use a more solid and safe language to access the game. 

This library also includes or allows to *hook* itself to the game at *runtime* allowing to use just **C#** as the scripting language.

## What's out of the box?

You'll have the capabilities to *open* and *edit* or *read* the contents of any resource that resides in the game and at the same time using the same library as an intermediary for the **game's backend scripting language**.

### Notes on the support

This library **is only supported** with the GTA:SA game and it may not work with older titles.

#### Offerings

Besides of the resource management and scripting, the library has a **built-in thread management** that allows to use, if needed, *asynchronous parallelism* during read and write operations for better performance. This allows the creator to do multiple tasks meanwhile doing others. 

Talking about the scripting side, some diassemblers used for the SCM language doesn't know how to approach **native data** from the engine preventing the creator to have a more *in-depth* access to the game logic. This library offers a ready-to-use diassembler for the SCM language that not only reverses the code but *allows to handle* the native data. 

The library is on a costant update and always announces changes and issues allowing the creator to say up-to-date!

## How it works

What you're going to read here is just a brief description of how everything works, for more information **visit the wiki**!

### The IMG Archive

Rockstar's first approach to data archiviation was a separation between a **directory entry** and a **image entry**, this meas that the directory would contain *all the contents needed* meanwhile the *image leads to them*. Apparently a **compression** approach was also taken to decrease the archive size, it was similiar to the [lzo](https://it.wikipedia.org/wiki/Lempel%E2%80%93Ziv%E2%80%93Oberhumer) standard back at the time. 

With San Andreas the story is almost the same, the directory entry and the raw data concept with the, before mentioned, compression **are combined**. In fact the archive contains a **header**, it contains the data needed to know *what the game is dealing with*. After that the **entries** come in, these are from the directory concept. Every directory, also called directory block, contains *specific data that indicates it's data offsets, size(s) and namings*. 

The archive is structured like above:

##### Header 

| **Size (bytes)** 	| **Type**    	| **Brief Description**                                           	|
|-----------------	|-------------	|-----------------------------------------------------------------	|
| ```4```           | ```char[4]``` | *Defines the version, for san andreas would be "VER2"*      	    |
| ```4```           | ```dword```   | *Defines the total count of entries available in the archive* 	  |

A quick example, reading the raw data of the script.img file that resides in 'data/scripts' at the header position you would find 

- ```0x56 0x45 0x52 0x32``` - In ascii code, it means ```VER2``` (These are the first 4 bytes).
- ```0x4F 0x00 0x00 0x00``` - The total count of entries in the archive (So these ones are the next 4 bytes after the version mark)

##### Directory Entry (Block)

| **Size (bytes)** 	      | **Type**        | **Brief Description**           |
|-----------------	      |----------	      |--------------------------------	|
| ```4```            	    | ```dword```    	| *Offset of the entry*         	|
| ```2```            	    | ```word```     	| *Streaming size of the entry* 	|
| ```2```            	    | ```word```     	| *Entry size in the archive*   	|
| ```24```           	    | ```char[24]``` 	| *Name of the file (nullable)* 	|

This block of data can vary based on the entry that you would find. The offset usually means where the data of the block resides meanwhile the streaming size would be the data that the entry contains.
