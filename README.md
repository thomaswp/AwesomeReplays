AwesomeReplays
==============

A replay parser for Awesomenauts. Currently supports reading .blockData files and displaying the paths of each character during that time. Note, the accuracy is still not perfect, and some paths are incorrect/noise.

Running the Parser
------------------
If you want to play with the parser without building it, simply download the lastest stable version [here](/build/AwesomeReplays.exe).

Building the Parser
-------------------
* Download and install [Visual Studio Desktop Express 2013](http://www.visualstudio.com/downloads/download-visual-studio-vs#d-express-windows-desktop)
* Open the .sln file in the root directory
* It should work without problem

Format
------
[This is a good post](http://www.awesomenauts.com/forum/viewtopic.php?f=19&t=29690) to start with for understanding what we know about the format, but below you'll find more specific info.

Each .blockData file consists of all the data needed to replay ~10s of game, in a mostly deterministic manner. The main structure of the .blockData files are as follows:
* A brief header of unknown format
* A series of blocks, one per character, that detail that character's movement and activity for the 10 seconds
* One or more sections follow this, of unknown format
 
One confusing thing about working with bit-packed data is that while humans read and write binary the same way they do decimal, with the most significant bit on the left, the most significant bit in block data will be written last (as would be expected). This means, if you imagine the data one giant binary string (0010101010110...) you would read it from right to left. 

### Character Blocks ###
The character blocks have the following format:
* A header, as short as 61 bits, but often much longer, of unknown format
* The ASCII-encoded name of the player, or an empty string if it is not a player-character. This could be a steam handle or a bot name. The strings, empty or not, terminate in a 0x00 byte
* The ASCII-encoded name of the character's class. Their corresponding characters can be found [here](http://joostdevblog.blogspot.com/2014/06/the-ai-tools-for-awesomenauts.html). As far as we can tell, these are the only two (ASCII-encoded) strings in the file
* What we're (possibly naively) dubbing the Ability block, of highly varying length - see below for more
* An 18-bit id, which seems to be constant across different .blockData files for a given replay
* Structured movement data (yay structure!) - see below for more
* One or more sections of unknown format

#### Ability Blocks ####
These blocks of at least 106 bits are so-named because they seem to increase in size dramatically only after abilities are purchased and used. They easily could store non-ability data as well.

Their format is difficult to parse, but has clear patterns and sections; however, these rules seem to have numerous exceptions. Here is the general pattern:
* 23 bits, eg: 11010100000000000000011
* 8 bits - if the most significant 4 of these are zero, skip the next two bullets, eg: 00001100
* 5 bits, followed by an arbitrary number of 23-bit sections
* possibly a repeat of the previous section
* 23 bits, which seem to always be in the form xxxxxxxx000000000000011, where the x's are the the same for a given character throughout the replay
* 8 bits - if the most significant 4 of these are zero, skip the next bullet
* An arbitrary number of 11-bit blocks
* 1111111000000000000011 - happens twice if the previous section was skipped

#### Structured Movement Data ####
* 7 bits, interpreted as a number of movement segments, which are each:
  * 9 bits, interpreted as the time this movement happened
  * 13 bits, interpreted as an x-coordinate
  * 13 bits, interpreted as a y-coordinate
