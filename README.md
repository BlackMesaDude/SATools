# SATools <img src="https://img.shields.io/badge/Version-1.0-informational" /> <img src="https://img.shields.io/badge/License-GPL%20v2.0-informational" />

## Index
<!--ts-->
   * [Introduction](#Introduction)
      * [Archive Manipulation](#archive-manipulation)
   * [ToDo](#todo)
<!--te-->

**SATools** (or *San Andreas Tools*) is a ```C#.Net``` based library made for the next mod developer that wants an easy way to access **GTA:SA resources**, such as archives or backend scipting files.

## Archive Manipulation

Rockstar approach to *data archiviation* with San Andreas is pretty similiar to *Liberty City* and *Vice City* approach, with only a difference. Instead of using **TXD compression** (similiar to the ```Izo``` approach) it was changed with the usage of ```directory entries + raw data```. In fact this version of the archive shows a **header** (that defines version and how many entries are stored) and each **directory entry block**.

### Header

| **Size (bytes)** 	| **Type**    	| **Brief Description**                                           	|
|--------------	|---------	|-------------------------------------------------------------	|
| ```4```            	| ```char[4]``` 	| *Defines the version, usually the value would be "VER2"*      	|
| ```4```            	| ```dword```   	| *Defines the total count of entries available in the archive* 	|

#### Header: Example

The table below represents the header values for the ```script.img```

| **0x00** 	| **0x01** 	| **0x02** 	| **0x03** 	| **0x04** 	| **0x05** 	| **0x06** 	| **0x07** 	| **...** 	|
|-----------	|-----------	|-----------	|------------	|-----------	|-----------	|-----------	|-----------	|-----------	|
| ```0x56```  | ```0x45``` 	| ```0x52``` 	| ```0x32``` 	| ```0x4F``` 	| ```0x00``` 	| ```0x00``` 	| ```0x00``` 	| ```...``` 	|

where from column *0x00* to *0x03* would be the 4 chars for the version definition meanwhile *0x04* to *0x07* the total count of entries available in the archive.

### Entry Block 

| **Size (bytes)** 	      | **Type**        | **Brief Description**           |
|-----------------	      |----------	      |--------------------------------	|
| ```4```            	    | ```dword```    	| *Offset of the entry*         	|
| ```2```            	    | ```word```     	| *Streaming size of the entry* 	|
| ```2```            	    | ```word```     	| *Entry size in the archive*   	|
| ```24```           	    | ```char[24]``` 	| *Name of the file (nullable)* 	|

# ToDo

- [x] Implement IMG manipulation
- [ ] Implement SCM manipulation
- [ ] Allow CLR hook to the unmanaged side of the game
