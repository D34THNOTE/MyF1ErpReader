Ego-Engine-Modding
==================

A suite of modding applications for the Ego Engine which powers Codemasters' racing game franchises (Dirt, Grid, and Formula 1).

The project was started in 2009/2010 with the Ego Database Editor for the game Race Driver: Grid when I was just starting to learn programming in High School. It has since been continuously updated to include many other tools.

The main solution is in the EgoEngineLibrary folder. Releases can be found [here](https://ryder25.itch.io/).

- **Ego CTF Editor** With this program you can edit CarTuningFile (CTF,CSV) files for the Ego Engine games. These files contain car performance data.
- **Ego Database Editor** This program will allow you to edit the database.bin file for the Ego Engine games. In the file you can edit tons of data like events, ai skill levels, vehicles, teams and so much more.
- **Ego ERP Archiver** This program lets you export and import resources from ERP archives for EGO engine games. The archives store many different types of data, including textures.
- **Ego File Converter** This program lets you convert binary XML, Lng, and Pkg files to human readable formats for EGO engine games.
- **Ego JPK Archiver** Open Jpk archives to export and import files from Ego Engine games. The raceload.jpk file contains physics, damage, and game loading data.
- **Ego Language Editor** This program provides a nice GUI for editing language files for Ego Engine games.
- **Ego PSSG Editor** This tool will allow you to open and edit PSSG files for EGO engine games. These files contain lots of data, including textures.



Above is the README of the original project which can be found [here](https://github.com/EgoEngineModding/Ego-Engine-Modding/tree/master)
=========================================================================================================================================


I have added a project I named "MyF1ErpReader" to it. It is my sandbox where I try to figure out how the original EgoErpArchiver application works, specifically how it is able to read the .erp file used in the F1 games from Codemasters, and how it is able to extract the XML file with tyre info from that .erp. My goal is to create a simple console application which will utilize the EgoErpLibrary, and a few classes from EgoErpArchiver modified to my needs, in order to print out a neatly formatted info about different tyre compounds which are present in that XML file encoded into the .erp.

My goal is to introduce formatting which provides information about which tyre compound the statistics are for, instead of using the names from the XML file like SUP(F or R standing for "front" and "rear") displaying their real-life and in-game names like C4, C3 etc. I plan on adding a feature where it gatheres all the data from the .erp file we passed to it and creates a text file with neatly formatted information, such as percentage of grip based on tyre wear and other statistics
