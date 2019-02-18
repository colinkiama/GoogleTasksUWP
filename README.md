# GoogleTasksUWP
Google Tasks UWP API

## How to install:

Available on Nuget:

Use command: `Install-Package GoogleTasksUWP.ColinKiama`

or search for `GoogleTasksUWP.ColinKiama`

## Progress:
An UWP Implementation of the Google Tasks API: https://developers.google.com/tasks/v1/reference/

Progress so far:
- [X] OAuth
- [X] Tasklists
- [X] Tasks
- [ ] Query Options

## How to use:
1. Follow instructions from "using your own credentials" https://github.com/googlesamples/oauth-apps-for-windows/tree/master/OAuthUniversalApp#using-your-own-credentials

2. Use GTasksOauth to start an authorisation request

3. Handle the Uri Callback with GTasksOauth to generate a token.

4. Use the token to create a GTasksClient

5. Start making requests.
