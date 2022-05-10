# Garmin_To_Fitbit_Steps_Sync_Web

## Project Resources

### Puppeteer
https://stackoverflow.com/questions/65049531/puppeteer-iframe-contentframe-returns-null
https://www.puppeteersharp.com/examples/index.html

### Garmin
C:\src\gh\garmin-connect-scraper\lib\main.js

### User Secrets / Configuration in a .NET 6 Consoel App
https://makolyte.com/how-to-add-user-secrets-in-a-dotnetcore-console-app/
https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration

### Using a Generic Host
https://dfederm.com/building-a-console-app-with-.net-generic-host/

### FitBit API Docs
https://dev.fitbit.com/build/reference/web-api/

## Development Roadmap
* Add Tip Jar
* Check to make sure that secrets aren't in the repo 

## Possible Features
* Make it visible to user if activities have been input for the previous days, so we don't forget or create a duplicate entry
* Calcuate Our Average for the Month (using our best 21 days)  - Could we calculate this as [Need to Average x steps over the next x days] 157,500 steps in a month

## Recently Completed Features
* Give us our average steps for the last 7 days here in the APP
* Need to have confidence in this app to input data into FitBit Correctly so that we don't ever have to login on the fitbit homepage
* Need to implement Azure keyvault - Currently Azure WebApp cannot access app secrets
* \[Useability\] Only Show the Add Activity and other buttons when we are already connected to FitBit.

### 08/12/2020
adding a drop-down to select the day for the activity. We usually input our steps for the previous day, so this will be the default. We will also provide an option to put in
the day before yesterday's step count.

### 09/13/2020
* Created README.md

### 09/18/2020
* Experimenting with activity time series. It would be helpful to get a snapshop of the last 7 days of steps
* Hide the Connect to FitBit button if we are already connected and don't show the functionality buttons if we are connected. Once we are connected, show the buttons!

### 10/09/2020
* We keep having to reconnect to the API after every request. Find a way to keep token between requets

### 10/25/2020
* Implement logging to check for successful logins
 
### 05/01/2022
* Next Time, get the path right for the steps dev in garmin connect , and get it writing to the text file correctly.

### 05/06/2022
* Get the selector correct for the Garmin Connect web scrape

### 05/07/2022
* Run the Garmin Step Reader successfully in headless mode

### 05/09/2022
* Start planning how to push data automatically to FitBit
Need to change FitBit API Redirect back to 
https://fitbitapitools.azurewebsites.net/authorised
https://dev.fitbit.com/apps/details/22BNTM

* Next time need to come back and build the Create Steps in the FitBitAPI class... 
* We will call this from the WebScraper with the days steps
* We may initialized it with the hard-coded access_code perhpas in user-secret vault - that is our next step and our next stream.





