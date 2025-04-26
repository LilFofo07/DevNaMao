"use client"

import { useEffect, useRef } from "react"
import QRCodeLibrary from "qrcode"

interface QRCodeProps {
  url: string
  size?: number
}

export default function QRCode({ url, size = 200 }: QRCodeProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null)

  useEffect(() => {
    if (canvasRef.current) {
      QRCodeLibrary.toCanvas(
        canvasRef.current,
        url,
        {
          width: size,
          margin: 1,
          color: {
            dark: "#000000",
            light: "#FFFFFF",
          },
        },
        (error) => {
          if (error) console.error("Erro ao gerar QR code:", error)
        },
      )
    }
  }, [url, size])

  return <canvas ref={canvasRef} width={size} height={size} />
}
