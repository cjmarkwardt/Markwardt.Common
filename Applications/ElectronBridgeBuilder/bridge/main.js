import { app, BrowserWindow, ipcMain } from 'electron'
import { Buffer } from "buffer"
import { join, dirname } from 'path'
import { createConnection } from "net"
import { fileURLToPath } from 'url'

process.on('uncaughtException', function (error) {
  console.error(('<~%FRONT-CRASH%~>' + error.stack + '\n-----------------------').replace(/(\r\n|\n|\r)/g, "<~%N%~>"))
})

const config = JSON.parse(process.argv[1])

const client = createConnection({ port: config.port }, () => {
  app.on('before-quit', event => {
    event.preventDefault()
    console.error('<~%FRONT-EXIT%~>')
  })

  app.whenReady().then(async () => {
    let window = new BrowserWindow({
      width: config.openOptions.width,
      height: config.openOptions.height,
      show: false,
      webPreferences: {
        nodeIntegration: false,
        contextIsolation: true,
        enableRemoteModule: false,
        preload: join(dirname(fileURLToPath(import.meta.url)), 'preload.js')
      }
    })

    window.setMenuBarVisibility(false)

    let devToolsShortcut = config.openOptions.enableDevToolsShortcut
    let fullscreenShortcut = config.openOptions.enableFullscreenShortcut

    window.webContents.on('before-input-event', (event, input) => {
      if (!devToolsShortcut && input.key.toLowerCase() === 'i' && ((input.control && input.shift) || (input.meta && input.alt))) {
        event.preventDefault()
      } else if (!fullscreenShortcut && input.key.toLowerCase() == 'f11') {
        event.preventDefault()
      }
    })

    function send(message) {
      const dataBuffer = Buffer.from(JSON.stringify(message))

      const lengthBuffer = Buffer.alloc(4)
      lengthBuffer.writeInt32LE(dataBuffer.length)
      client.write(lengthBuffer)

      client.write(dataBuffer)
    }

    ipcMain.on('toBackend', (_, message) => send(message))
    ipcMain.on('setWidth', (_, value) => window.setSize(value, window.getSize()[1]))
    ipcMain.on('setHeight', (_, value) => window.setSize(window.getSize()[0], value))
    ipcMain.on('setDevTools', (_, value) => value ? window.webContents.openDevTools() : window.webContents.closeDevTools())
    ipcMain.on('setDevToolsShortcut', (_, value) => devToolsShortcut = value)
    ipcMain.on('setFullscreen', (_, value) => window.fullScreen = value)
    ipcMain.on('setFullscreenShortcut', (_, value) => fullscreenShortcut = value)
    ipcMain.on('setMaximized', (_, value) => value ? window.maximize() : window.unmaximize())
    ipcMain.on('setMinimized', (_, value) => value ? window.minimize() : window.restore())
    
    let receiveBuffer = Buffer.alloc(0)

    let lastReceived = null
    setInterval(() => {
      send({})

      if (lastReceived !== null && Date.now() - lastReceived > 3000) {
        app.quit()
      }
    }, 1000);

    client.on('readable', () => {
      if (window === null) {
        return
      }

      let chunk;
      while (null !== (chunk = client.read())) {
        receiveBuffer = Buffer.concat([receiveBuffer, chunk])
        while (receiveBuffer.length >= 4) {
          const length = receiveBuffer.readInt32LE(0)
          
          if (receiveBuffer.length < 4 + length) {
            break;
          }

          lastReceived = Date.now()

          const messageData = receiveBuffer.subarray(4, 4 + length)
          receiveBuffer = receiveBuffer.subarray(4 + length)
          const message = JSON.parse(messageData)

          if (message.content !== undefined) {
            window.webContents.send('toFrontend', message)
          }
        }
      }
    })

    await window.loadURL(config.location)
    window.show()
    await new Promise(resolve => setTimeout(resolve, 50));

    if (config.openOptions.isMaximized) {
      window.maximize()
    }
    
    if (config.openOptions.isFullscreen) {
      window.fullScreen = true
    }

    if (config.openOptions.showDevTools) {
      window.webContents.openDevTools()
    }
  })
});