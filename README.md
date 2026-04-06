# IoList Wallet

> Searchable-List based GUI for UTxO/eUTxO blockchain wallets, full coin control.

## Structure

Every IoPanel (root content inside a window/dock) in the app is either a IoBook or a IoList:

### IoBook
- Has page tabs at the top, and can also swipe between the tabs horizontally with drag gestures.
- Swiping up, down, or to previous of the first page, will close the IoBook.
- Each page can contain a list of custom rows, typically Settings, or a single-row fullscreen IoList.
- Used for Settings of anything, or a BookFilter, or IoProcess.

### IoList
- Has a toolbar on the top (windowed) or bottom (fullscreen)
- The toolbar contains a page select, page size scaler (entries per page), and optionally extra custom buttons.
- The pages can also be swiped vertically (like how IoBook pages are swiped horizontally).
- Each IoList comes with a retractable BookFilter with filtering options.
- Used for selectors and list-viewers of anything, like: Main Menu, Transaction History, UTxOs, ...

### BookFilter
- A special IoBook that comes included with every List as a retractable panel.
- Can be toggle by clicking the Filter button, or (if the IoList is not inside an IoBook) by swiping the list horizontally.
- First page lets you chose how to sort (two levels or sorting, the secondary will sort the pockets or identically primary sorted entries).
- Further pages are searchign conditions, if the entry will satisfy at least one tab (tabs are OR), it will be shown.
- Each tab searches within any of the enabled properties of the entry, a tab will be satisfied if all enabled properties in that tab match.
- Bottom text shows the result counter: S - F / T. Where S is shown entries, F is filtered out entries, T is total entries (=S+F).

### IoProcess
- An IoBook overriding the content of the parent panel, with a modal process, where you have to go through some steps to do something.
- When failed, successful, or went back, the parent content will activate back.
- Build Tx: 1. Lacking inputs (add auto/manually) 2. Lacking change address (select account/customaddress), 3. Withdraw rewards?, ...
- Add Wallet: 1. How (restore/new/import/HW/multisig/readonly, ...), 2. further stuff like derivation, seed words etc.
- Add account: 1. Index + Name.

## Features
- Multi-platform build in Avalonia. **(NOT TESTED YET BEYOND WINDOWS)**
- Multi-lingual support, with a secondary layer where you can replace some texts with symbols.
- Multi-profile support - each profile is a completely separate save folder, with it's own networks, app settings, password, etc.
- Multi-chain support. **(NO CHAINS FULLY IMPLEMENTED YET)**
- Multi-wallet and multi-account support. **(NOT FULLY IMPLEMENTED YET)**
- Profile encryption - if you add password to a profile, it will encrypt everything beyond the basic profile settings with that password.
- Extensive customizability.
- Display modes - fullscreen, windowed, multi-window (multi-window opens each IoPanel in a separate window).
- Docking multiple IoPanels into one window **(NOT FULLY IMPLEMENTED YET)**
- Adaptive to any resolution or control scheme.
- Pinnable IoPanels - non-modal IoPanels can toggle being pinned at the bottom toolbar, then found in the Pinned list for fast travel.
- Anything can have a tooltip with extra info/instructions, when hovered over with the cursor.
- Permanency - any closed screen (if not specifically **DELETED**) wil lreopen with previous state, even after restarting the app!
- Complex address books.
- Complex filtering of any IoList, and EVERYTHING that can be expressed as a list is an IoList.
- Complex TxBuilder - allows to batch any set of all kinds of actions into a single transaction. **(NOT IMPLEMENTED YET)**
- Rebroadcasting - edit the pending tranasction if it is not on-chain yet. Redirect bad tx, bump fee, ... **(NOT IMPLEMENTED YET)**
- Automatic and manual transaction input selection **(NOT IMPLEMENTED YET)**
- Peer-2-Peer DeFi for smart chains **(NOT IMPLEMENTED YET)**
- Multi-delegation for dPOS chains **(NOT IMPLEMENTED YET)**
