# IoList Wallet  
  
> Searchable-List-based GUI for UTxO/eUTxO blockchain wallets, full coin control.  

** Status
-Currently 0 funding, and I can't continue working on this for free any longer. I hope I get even a tiny amount of funding for this project.
-GUI level parts and profiles are 99.9% finished. Content Windows can not be built with ease and support everything I've coded.
-There is only 1 last structure I'd need to make more advanced, before anything I could imagine with wallet stuff will begin to be easily buildable on top of it.
-Any willing help for this project will be greatly appreciated

These are some small examples how it works and looks like:
<img width="1422" height="951" alt="io2" src="https://github.com/user-attachments/assets/aad7027e-f968-4e91-91f2-d49ec5c14adb" />
<img width="1405" height="1599" alt="io1" src="https://github.com/user-attachments/assets/62c0cb50-70f6-413e-bb8f-d61850bbb0c6" />
<img width="1433" height="1072" alt="io3" src="https://github.com/user-attachments/assets/99cbee01-414e-4ff9-8be8-fd3590c7ee42" />
<img width="1726" height="851" alt="io4" src="https://github.com/user-attachments/assets/9a0ef262-ed0d-48a2-8845-f3cfe108040a" />

## Structure  
  
Every IoPanel (root content inside a window/dock) in the app is either an IoBook or an IoList:  
  
### IoBook  
Has page tabs at the top, and can also swipe between the tabs horizontally with drag gestures.  
Swiping up, down, or to the left of the first page will close the IoBook.  
Each page can contain a list of custom rows, typically Settings, or a single-row fullscreen IoList.  
Examples: Settings/Edits of anything that isn't a list of similar items + specials like BookFilter, or IoProcess...  
  
### IoList  
Has a toolbar on the top (windowed) or bottom (fullscreen)  
The toolbar contains a page select, page size scaler (entries per page), and optionally extra custom buttons.  
The pages can also be swiped vertically (like how IoBook pages are swiped horizontally).  
Each IoList comes with a retractable BookFilter with filtering options.  
Examples:  Selectors and list-viewers of anything, like: Main Menu, Transaction History, UTxOs, ...  
  
### BookFilter  
A special IoBook that comes included with every List as a retractable panel.  
Can be toggled by clicking the Filter button, or (if the IoList is not inside an IoBook) by swiping the list horizontally.  
First page lets you choose how to sort (two levels of sorting, the secondary will sort the pockets or identically primary sorted entries).  
Further pages are searching conditions, if the entry satisfies at least one tab (tabs are OR), it will be shown.  
Each tab searches within any of the enabled properties of the entry. A tab will be satisfied if all enabled properties in that tab match.  
Bottom shows the result counter: S - F / T. Where S are the shown entries, F are the filtered out entries, T are the total entries (=S+F).  
  
### IoProcess  
A special modal IoBook, that swipes pages with the back and comfirm button instead of freely...  
...where you have to go through some steps to perform some process.  
When failed, successful, or went back, it closes bback to the parent panel.  
Examples:  
Build Tx: Wrong inputs? -> Lacking inputs? (add auto/manually) -> No change address? (select account/customAddress) -> Withdraw rewards?, ...  
Add Wallet: How (restore/new/import/HW/multisig/readOnly, ...) -> Further stuff like derivation, seed words, etc.  
Add account: Indices + Names + which to select.  
  
## Features
- Multi-platform build in Avalonia. **(NOT TESTED BEYOND WINDOWS YET, BUT WILL PROBABLY RUN OK)**  
- Multi-lingual support, with a secondary layer where you can replace some texts with symbols.  
- Multi-profile support - each profile is a completely separate save folder, with its own networks, app settings, password, etc.  
- Multi-chain support. **(NO CHAINS FULLY IMPLEMENTED YET)**  
- Multi-wallet and multi-account support. **(~70% IMPLEMENTED)**  
- Profile encryption - if you add a password to a profile, it will encrypt everything beyond the basic profile settings with that password.  
- Extensive customizability.  
- Display modes - fullscreen, windowed, multi-window (multi-window opens each IoPanel in a separate window).  
- Docking multiple IoPanels into one window **(~70% IMPLEMENTED)**  
- Adaptive to any resolution or control scheme.  
- Pinnable IoPanels - non-modal IoPanels can toggle being pinned at the bottom toolbar, then found in the Pinned list for fast travel.  
- Anything can have a tooltip with extra info/instructions when hovered over with the cursor.  
- Permanency - any closed screen (if not specifically **REMOVED**) will reopen with the previous state, even after restarting the app!  
- Complex address books.  
- Complex filtering of any IoList, and EVERYTHING that can be expressed as a list is an IoList.  
- Complex TxBuilder - allows for batch any set of all kinds of actions into a single transaction. **(NOT IMPLEMENTED YET)**  
- Rebroadcasting - edit the pending transaction if it is not on-chain yet. Redirect bad tx, bump fee, ... **(NOT IMPLEMENTED YET)**  
- Automatic and manual transaction input selection **(NOT IMPLEMENTED YET)**  
### Wishful features   
- Multi-delegation for dPOS chains **(I HOPE, NOT IMPLEMENTED YET)**  
- Peer-2-Peer DeFi for smart chains **(I HOPE, NOT IMPLEMENTED YET)**  
  
# How to use  
  
The app has a layered design. The home page has one row for each layer.  
**App** -> Profiles[] *(needs a login)*  
**Profile** -> Networks[] *(needs a profile)*  
**Network** -> Transactions[] + Wallets[] *(need a network)*  
**Wallet** -> Accounts[]  *(needs a wallet)*  
**Account** -> UTxOs[] *(needs an account)*  
**Transaction Builder** *(needs UTxOs to pay for them)*  
  
Here are the layered steps of the wallet usage. The first 4 can be only initial for casual users.  
So that after the setup they could even go to the Main Menu filter and hide those network/wallet layers.  
Also the app will do its best to actively push and guide you to initialize these steps, if they are not prepared yet.  
  
## 1. Log in to your profile (if not already logged in)  
The app will create and select a profile if there isn't one.  
The first row on the home screen is the profile selection and login/settings.  
At the startup, the last profile you were in will be selected.  
But if you have encrypted it with a password in its settings, you will need to input it to access the profile.  
The app starts at the home screen if it already opens logged in (when there's no password), otherwise, it opens at the login page.  
To the right of the login page, you can set up your app.  
Or swipe in any other direction, or click Back, or close a multiwindow to go back to the home screen.  
  
## 2. Select a network (if not already selected)  
The second row on the home screen is the network selection.  
At the startup, the last network you were in will be selected.  
You will be prompted to add a network if there's none. You can also add another network by clicking the Add Network button.  
The network settings are accessible with the little button to the right of the network rows.  
To the right in the network settings, you can set up your network or find a list of your wallets or all network transactions.  
Or swipe in any other direction, or click Back, or close a multiwindow to go back to the home screen.  
  
## 3. Make/Select a wallet (if not already selected)  
The third row on the home screen is the wallet selection.  
At the startup, the last wallet you were in will be selected.  
You will be prompted to make a wallet if there's none. You can also make another wallet by clicking the Add Wallet button.  
The wallet settings are accessible with the little button to the right of the wallet rows.  
To the right in the wallet settings, you can set up your wallet or access accounts.  
Or swipe in any other direction, or click Back, or close a multiwindow to go back to the home screen.  
  
## 4. Make/Select an account (if not already selected)  
The fourth row on the home screen is the account selection.  
At the startup, the last account you were in will be selected.  
You will be prompted to make an account if there's none. You can also make another account by clicking the Add Account button.  
When creating an account, you can also type in more indices to make more of them at once. O even "A-B" to make them from A to B.  
The account settings are accessible with the little button to the right of the account rows.  
To the right in the account settings, you can set up your account and see what's there.  
Or swipe in any other direction, or click Back, or close a multiwindow to go back to the home screen.  
  
## 5. Formulate a transaction  
The fifth row on the home screen is the transaction builder.  
In the overview page, you will see the big picture...  
...like what total of any coins or tokens are sent from which accounts/wallets, to which recipients. You also sign and broadcast here.  
On the outputs page, you will build the transaction contents. Keep adding outputs as actions you want to perform.  
After everything you want to happen is in the outputs, you can go to the overview and build-sign-broadcast.  
Or you can go to the inputs tab and add specific collections of UTxOs you want to spend.  
Building the transaction in the overview will report any errors, warnings, or ask you to fill in anything missing, or do so automatically.  
For non-output actions like rewards, delegate, vote, etc., you will find those in the account tabs (the Tx overview can also lead there)  

## 6. Build, Sign and Broadcast the transaction  
This is done in the overview page, after reading the overview to make sure what you are doing, you click the Build button.  
It will go through all the chacks, like checking valid inputs, balancing the transaction and selecting change. Finally computes the fee.  
When it's built, you will be able to sign. It will open a list of required signatures and lets you sign them all, one by one, or in batch.  
When it's signed you can broadcast. Or you can make any edits, which will reset the build, and deprecate the signatures.  
You could also edit it after broadcasting, as long as it's not on chain yet, you might re-broadcast edits, with a bumped fee if possible.  
A simple mode will merge the three Processes into one.  
  
# How to send ALL?  
This is a great question that might appear puzzling at first, as there's no simple "Send all" button...  
...but if you understand how the wallet works, IoList's way to do this is quite interesting, and still very easy!  
(but there will be big warnings and pushing the smaller of the buttons a few times, so don't worry of doing it by accident).  
You will really need to use the intended backwards path: Make a fresh transaction with no outputs, and immediately try to build it.  
It will ask for input to pay for the small transaction fee, and you will click to select manually.  
On the UTxO selection page, open the filter, and make sure it shows all your accounts you want to send all from, with no filters.  
Click "Select All". The builder is now satisfied with the inputs, as long as you actually have any coins to even pay the fee.  
Then it will ask if you want to add more inputs to the selection or confirm the selection.  
Then it will ask you where to send all that 99.999% change back to, and manually select a custom recipient for the change.  
The app will tell you you are sending all change away, and if you are really sure, confirm.  
If it is at least 10% of all your coins, it will again tell you that you are sending a lot of coins away, and ask again.  
After all the - are you sures... if no errors, the "Send All" transaction is ready for signing and broadcasting.  
