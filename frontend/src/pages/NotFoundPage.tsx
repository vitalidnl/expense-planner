import { Link } from 'react-router-dom'

export default function NotFoundPage() {
  return (
    <section className="space-y-2">
      <h1 className="text-xl font-semibold">Not Found</h1>
      <p className="text-sm">That route does not exist.</p>
      <Link className="underline" to="/">
        Go home
      </Link>
    </section>
  )
}
