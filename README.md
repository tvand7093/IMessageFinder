# iMessageFinder
A simple cross platfrom CLI tool for pulling text messages and saving them to a file.

###Important Note
Currently this program only supports when a single device is backed up to the computer. 

###Quick Start
To run, simply specify a phone number arguement: `./IMessageFinder.exe 1234567890`. It is recomended you take the steps below first otherwise you won't get the most current messages.

###Steps to run for most current messages
1. First plug in your iDevice and open up iTunes.
2. Make a local computer copy backup of your iDevice.
3. Open up the command line/terminal and navigate to the binary file produced by this project.
3. If not on Windows, you must run the program under Mono like so: `mono ./IMessageFinder.exe <insert your number here>`. If on windows, just navigate to the file and open it with the same commands.
 - As a side note, the number you input must include the zip code and only be 10 characters long.
 4. The application will place a file on your desktop with the text messages found that match the specified phone number.
