import { Link } from 'react-router-dom'
import { useEffect, useState } from 'react'

const API = '/api'

export default function ToursPage() {
  const [tours, setTours] = useState([])

  useEffect(() => {
    fetch(`${API}/tours`).then((r) => r.json()).then(setTours)
  }, [])

  return (
    <section>
      <h2>Выбор тура</h2>
      <div className="grid">
        {tours.map((tour) => (
          <article key={tour.id} className="card">
            <h3>{tour.title}</h3>
            <p>{tour.country}</p>
            <p>{tour.durationDays} дней</p>
            <strong>{tour.price} €</strong>
            <Link to={`/tour/${tour.id}`}>Подробнее</Link>
          </article>
        ))}
      </div>
    </section>
  )
}
