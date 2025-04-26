import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import QRCode from "@/components/qr-code"

export default function Home() {
  // Obter a URL atual do site (funciona tanto em desenvolvimento quanto em produção)
  const baseUrl =
    process.env.NEXT_PUBLIC_SITE_URL ||
    (process.env.NEXT_PUBLIC_VERCEL_URL ? `https://${process.env.NEXT_PUBLIC_VERCEL_URL}` : "http://localhost:3000")

  const cardapioUrl = `${baseUrl}/cardapio`

  return (
    <div className="flex min-h-screen flex-col">
      <header className="bg-white border-b sticky top-0 z-10">
        <div className="container flex h-16 items-center justify-between px-4 md:px-6">
          <div className="flex items-center gap-2">
            <h1 className="text-xl font-bold">Restaurante Delícia</h1>
          </div>
          <nav className="flex gap-4">
            <Link href="/cardapio">
              <Button variant="ghost">Cardápio</Button>
            </Link>
            <Link href="/admin/login">
              <Button variant="outline">Área Admin</Button>
            </Link>
          </nav>
        </div>
      </header>
      <main className="flex-1">
        <section className="w-full py-12 md:py-24 lg:py-32">
          <div className="container px-4 md:px-6">
            <div className="grid gap-6 lg:grid-cols-2 lg:gap-12 items-center">
              <div className="space-y-4">
                <h2 className="text-3xl font-bold tracking-tighter sm:text-4xl md:text-5xl">Cardápio Digital</h2>
                <p className="text-gray-500 md:text-xl/relaxed lg:text-base/relaxed xl:text-xl/relaxed">
                  Escaneie o QR code ao lado para acessar nosso cardápio digital completo. Veja fotos, descrições e
                  preços de todos os nossos deliciosos lanches.
                </p>
                <div className="flex flex-col gap-2 min-[400px]:flex-row">
                  <Link href="/cardapio">
                    <Button>Ver Cardápio</Button>
                  </Link>
                  <Link href="/admin/login">
                    <Button variant="outline">Área Administrativa</Button>
                  </Link>
                </div>
              </div>
              <div className="flex justify-center">
                <Card className="w-full max-w-sm">
                  <CardContent className="p-6 flex flex-col items-center gap-4">
                    <h3 className="text-xl font-bold">Acesse nosso cardápio</h3>
                    <div className="border p-4 rounded-lg bg-white">
                      <QRCode url={cardapioUrl} size={200} />
                    </div>
                    <p className="text-sm text-center text-gray-500">
                      Escaneie este QR code com a câmera do seu celular
                    </p>
                    <p className="text-xs text-center text-gray-400 break-all">{cardapioUrl}</p>
                  </CardContent>
                </Card>
              </div>
            </div>
          </div>
        </section>
      </main>
      <footer className="border-t py-6 md:py-0">
        <div className="container flex flex-col items-center justify-between gap-4 md:h-24 md:flex-row px-4 md:px-6">
          <p className="text-center text-sm leading-loose text-gray-500 md:text-left">
            © 2024 Restaurante Delícia. Todos os direitos reservados.
          </p>
        </div>
      </footer>
    </div>
  )
}
